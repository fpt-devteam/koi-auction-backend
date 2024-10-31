const axios = require('axios').default; // npm install axios
const CryptoJS = require('crypto-js'); // npm install crypto-js
const moment = require('moment'); // npm install moment
const qs = require('qs'); // npm install qs
const { Op, NUMBER } = require('sequelize');
const User = require('../models/user');
const Transaction = require('../models/transaction');
const Wallet = require('../models/wallet');
const { RelationshipType } = require('sequelize/lib/errors/database/foreign-key-constraint-error');

const deposit = async (req, res) => {
   const { Amount } = req.body;
   const { UserId } = req.user;

   const config = {
      app_id: "2553",
      key1: "PcY4iZIKFCIdgZvA6ueMcMHHUbRLYjPL",
      key2: "kLtgPl8HHhfvMuDHPwKfgfsY4Ydm9eIz",
      endpoint: "https://sb-openapi.zalopay.vn/v2/create"
   };

   const embed_data = {
      redirecturl: 'localhost:5173'
   };
   const items = [{}];
   const transID = Math.floor(Math.random() * 1000000);
   const app_trans_id = `${moment().format('YYMMDD')}_${transID}`;
   const order = {
      app_id: config.app_id,
      app_trans_id: app_trans_id,
      app_user: "user123",
      app_time: Date.now(),
      item: JSON.stringify(items),
      embed_data: JSON.stringify(embed_data),
      amount: Amount,
      description: "Thanh toán hóa đơn",
      bank_code: "zalopayapp",
      callback_url: "https://b4e3-2401-d800-5aec-76ef-c58c-ff24-9016-45ef.ngrok-free.app/payment-service/callback",
   };

   console.log(`app_trans_id = ${app_trans_id}`);

   const data = config.app_id + "|" + order.app_trans_id + "|" + order.app_user + "|" + order.amount + "|" + order.app_time + "|" + order.embed_data + "|" + order.item;
   order.mac = CryptoJS.HmacSHA256(data, config.key1).toString();

   const response = await axios.post(config.endpoint, null, { params: order });

   const wallet = await Wallet.findOne({ where: { UserId: UserId } });

   try {
      await Transaction.create({
         UserId: UserId,
         Amount: Amount,
         WalletId: wallet.WalletId,
         StatusId: 1,
         TransTypeId: 3,
         AppTransId: app_trans_id,
         BalanceAfter: wallet.Balance + Amount,
         Note: "Thanh toán hóa đơn",
         CreatedAt: Date.now(),
      });
   } catch (err) {
      console.log(err);
   }

   res.status(200).json(response.data);
};

const callback = async (req, res) => {
   const config = {
      key2: "kLtgPl8HHhfvMuDHPwKfgfsY4Ydm9eIz"
   };

   let result = {};

   try {
      let dataStr = req.body.data;
      let reqMac = req.body.mac;

      console.log("data =", dataStr);

      let mac = CryptoJS.HmacSHA256(dataStr, config.key2).toString();
      console.log("mac =", mac);

      // kiểm tra callback hợp lệ (đến từ ZaloPay server)
      if (reqMac !== mac) {
         // callback không hợp lệ
         result.return_code = -1;
         result.return_message = "mac not equal";
      }
      else {
         // thanh toán thành công
         // merchant cập nhật trạng thái cho đơn hàng
         let dataJson = JSON.parse(dataStr, config.key2);
         console.log("update order's status = success where app_trans_id =", dataJson["app_trans_id"]);

         result.return_code = 1;
         result.return_message = "success";

         const appTransId = dataJson["app_trans_id"];

         const transaction = await Transaction.findOne({ where: { AppTransId: appTransId } });
         if (transaction) {
            try {
               await Transaction.update(
                  { StatusId: 2 },
                  { where: { AppTransId: appTransId } }
               );
               await Wallet.update(
                  { Balance: transaction.BalanceAfter },
                  { where: { WalletId: transaction.WalletId } }
               );
            } catch (err) {
               console.log(err);
            }
         }
      }
   } catch (ex) {
      result.return_code = 0; // ZaloPay server sẽ callback lại (tối đa 3 lần)
      result.return_message = ex.message;
   }

   // thông báo kết quả cho ZaloPay server
   res.status(200).json(result);
};

const checkOrderStatus = async (orderId) => {
   const config = {
      app_id: "2553",
      key1: "PcY4iZIKFCIdgZvA6ueMcMHHUbRLYjPL",
      key2: "kLtgPl8HHhfvMuDHPwKfgfsY4Ydm9eIz",
      endpoint: "https://sb-openapi.zalopay.vn/v2/query"
   };

   let postData = {
      app_id: config.app_id,
      app_trans_id: orderId, // Input your app_trans_id
   }

   let data = postData.app_id + "|" + postData.app_trans_id + "|" + config.key1; // appid|app_trans_id|key1
   postData.mac = CryptoJS.HmacSHA256(data, config.key1).toString();

   const response = await axios.post(config.endpoint, null, { params: postData });
   return response.data;
};

const refreshWallet = async (UserId) => {
   const wallet = await Wallet.findOne({ where: { UserId: UserId } });

   const transaction = await Transaction.findAll({
      where: {
         WalletId: wallet.WalletId,
         StatusId: 1,
      }
   });

   let balance = Number(wallet.Balance);

   for (const trans of transaction) {
      try {
         const status = await checkOrderStatus(trans.AppTransId);
         if (status.return_code === 1) {
            balance += trans.Amount;
            await Transaction.update(
               { StatusId: 2 },
               { where: { AppTransId: trans.AppTransId } }
            );
         } else if (status.return_code === 2) {
            await Transaction.update(
               { StatusId: 3 },
               { where: { AppTransId: trans.AppTransId } }
            );
         }
      } catch (err) {
         console.log(err);
         return { message: "Internal Server Error", status: 500 };
      }
   }

   await Wallet.update(
      { Balance: balance },
      { where: { WalletId: wallet.WalletId } }
   );

   return { 
      WalletId: wallet.WalletId,
      Balance: balance,
      Currency: wallet.Currency,
      status: 200
   };
}

const reloadWallet = async (req, res) => {
   const { UserId } = req.user;

   const wallet = await Wallet.findOne({ where: { UserId: UserId } });

   const transaction = await Transaction.findAll({
      where: {
         WalletId: wallet.WalletId,
         StatusId: 1,
      }
   });

   let balance = Number(wallet.Balance);

   for (const trans of transaction) {
      const status = await checkOrderStatus(trans.AppTransId);
      if (status.return_code === 1) {
         balance += trans.Amount;
         await Transaction.update(
            { StatusId: 2 },
            { where: { AppTransId: trans.AppTransId } }
         );
      } else if (status.return_code === 2) {
         await Transaction.update(
            { StatusId: 3 },
            { where: { AppTransId: trans.AppTransId } }
         );
      }
   }

   await Wallet.update(
      { Balance: balance },
      { where: { WalletId: wallet.WalletId } }
   );

   res.status(200).json({ message: "Update wallet successfully" });
}

const getWalletBalance = async (req, res) => {
   const { UserId } = req.user;

   const refresh = await refreshWallet(UserId);
   if (refresh.status === 500) {
      return res.status(500).json({ message: "Internal Server Error" });
   }

   res.status(200).json({ balance: refresh.Balance });
};

const getTransactionHistory = async (req, res) => {
   const { UserId } = req.user;

   try {
      const wallet = await Wallet.findOne({ where: { UserId: UserId } });

      const transaction = await Transaction.findAll({
         where: {
            WalletId: wallet.WalletId
         }
      });

      res.status(200).json(transaction);
   } catch (err) {
      console.log(err);
      return res.status(500).json({ message: "Internal Server Error" });
   }
}

const payment = async (req, res) => {
   const { Amount } = req.body;
   const { UserId } = req.user;

   if (!Amount) {
      return res.status(400).json({ message: "Amount is required" });
   }

   if (Amount <= 0) {
      return res.status(400).json({ message: "Amount must be greater than 0" });
   }

   const wallet = await Wallet.findOne({ where: { UserId: UserId } });

   if (wallet.Balance < Amount) {
      return res.status(400).json({ message: "Not enough money" });
   }

   try {
      await Transaction.create({
         UserId: UserId,
         Amount: Amount,
         WalletId: wallet.WalletId,
         StatusId: 2,
         TransTypeId: 2,
         BalanceAfter: wallet.Balance - Amount,
         Note: "Thanh toán hóa đơn",
         CreatedAt: Date.now(),
      });

      await Wallet.update(
         { Balance: wallet.Balance - Amount },
         { where: { WalletId: wallet.WalletId } }
      );
   } catch (err) {
      console.log(err);
   }

   res.status(200).json({ message: "Payment successfully" });
};

const getAllWalletBalance = async (req, res) => {
   let wallets = await Wallet.findAll();
   await Promise.all(wallets.map(async (wallet) => {
      const refresh = await refreshWallet(wallet.UserId);
      wallet.Balance = refresh.Balance;
   }));

   res.status(200).json(wallets);
}

const getWalletBalanceByUserId = async (req, res) => {
   const { UserId } = req.params;

   try {
      // const wallet = await Wallet.findOne({ where: { UserId: UserId } });
      const refresh = await refreshWallet(UserId);

      res.status(200).json({
         UserId: UserId,
         WalletId: refresh.WalletId,
         Balance: refresh.Balance,
         Currency: refresh.Currency,
      });

   } catch (err) {
      console.log(err);
      return res.status(500).json({ message: "Internal Server Error" });
   }
}

const getAllTransactionHistory = async (req, res) => {
   let transactions = await Transaction.findAll();

   res.status(200).json(transactions);
}

const getTransactionHistoryByUserId = async (req, res) => {
   const { UserId } = req.params;

   try {

      const wallet = await Wallet.findOne({ where: { UserId: UserId } });

      let transaction = await Transaction.findAll({
         where: {
            WalletId: wallet.WalletId
         }
      });

      res.status(200).json(transaction);
   } catch (err) {
      console.log(err);
      return res.status(500).json({ message: "Internal Server Error" });
   }
}

module.exports = {
   deposit,
   payment,
   callback,
   reloadWallet,
   getWalletBalance,
   getTransactionHistory,
   getAllWalletBalance,
   getWalletBalanceByUserId,
   getTransactionHistoryByUserId,
   getAllTransactionHistory,
};
