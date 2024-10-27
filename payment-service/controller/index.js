const axios = require('axios').default; // npm install axios
const CryptoJS = require('crypto-js'); // npm install crypto-js
const moment = require('moment'); // npm install moment
const qs = require('qs'); // npm install qs
const { Op } = require('sequelize');
const User = require('../models/user');
const Transaction = require('../models/transaction');
const Wallet = require('../models/wallet');

const deposit = async (req, res) => {
   const { amount } = req.body;
   const { UserId } = req.user;

   const config = {
      app_id: "2553",
      key1: "PcY4iZIKFCIdgZvA6ueMcMHHUbRLYjPL",
      key2: "kLtgPl8HHhfvMuDHPwKfgfsY4Ydm9eIz",
      endpoint: "https://sb-openapi.zalopay.vn/v2/create"
   };

   const embed_data = {};
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
      amount: amount,
      description: "Thanh toán hóa đơn",
      bank_code: "zalopayapp",
      callback_url: "https://0e47-2405-4802-a339-ffb0-ac95-2c83-34ec-e544.ngrok-free.app/payment-service/callback",
   };

   console.log(`app_trans_id = ${app_trans_id}`);

   const data = config.app_id + "|" + order.app_trans_id + "|" + order.app_user + "|" + order.amount + "|" + order.app_time + "|" + order.embed_data + "|" + order.item;
   order.mac = CryptoJS.HmacSHA256(data, config.key1).toString();

   const response = await axios.post(config.endpoint, null, { params: order });

   const wallet = await Wallet.findOne({ where: { UserId: UserId } });

   try {
      await Transaction.create({
         UserId: UserId,
         Amount: amount,
         WalletId: wallet.WalletId,
         StatusId: 1,
         TransTypeId: 3,
         AppTransId: app_trans_id,
         BalanceAfter: wallet.Balance + amount,
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

   const wallet = await Wallet.findOne({ where: { UserId: UserId } });

   res.status(200).json({ balance: wallet.Balance });
};

const getTransactionHistory = async (req, res) => {
   const { UserId } = req.user;

   const wallet = await Wallet.findOne({ where: { UserId: UserId } });

   const transaction = await Transaction.findAll({
      where: {
         WalletId: wallet.WalletId
      }
   });

   res.status(200).json(transaction);
}

const payment = async (req, res) => {
   const { amount } = req.body;
   const { UserId } = req.user;

   const wallet = await Wallet.findOne({ where: { UserId: UserId } });

   if (wallet.Balance < amount) {
      return res.status(400).json({ message: "Not enough money" });
   }

   try {
      await Transaction.create({
         UserId: UserId,
         Amount: amount,
         WalletId: wallet.WalletId,
         StatusId: 2,
         TransTypeId: 2,
         BalanceAfter: wallet.Balance - amount,
         Note: "Thanh toán hóa đơn",
         CreatedAt: Date.now(),
      });

      await Wallet.update(
         { Balance: wallet.Balance - amount },
         { where: { WalletId: wallet.WalletId } }
      );
   } catch (err) {
      console.log(err);
   }

   res.status(200).json({ message: "Payment successfully" });
};

const getAllWalletBalance = async (req, res) => {
   let wallets = await Wallet.findAll();
   wallets.map(wallet => {
      {
         wallet.UserId;
         wallet.WalletId;
         wallet.Balance;
         wallet.Currency;
      }
   })

   res.status(200).json(wallets);
}

const getWalletBalanceByUserId = async (req, res) => {
   const { WalletId } = req.params;

   const wallet = await Wallet.findOne({ where: { WalletId: WalletId } });

   res.status(200).json({
      UserId: wallet.UserId,
      WalletId: wallet.WalletId,
      Balance: wallet.Balance,
      Currency: wallet.Currency,
   });
}

const getAllTransactionHistory = async (req, res) => {
   let transactions = await Transaction.findAll();

   res.status(200).json(transactions);
}

const getTransactionHistoryByWalletId = async (req, res) => {
   const { WalletId } = req.params;

   let transaction = await Transaction.findAll({
      where: {
         WalletId: WalletId
      }
   });
   res.status(200).json(transaction);
}

const getTransactionHistoryByUserId = async (req, res) => {
   const { UserId } = req.params;

   const wallet = await Wallet.findOne({ where: { UserId: UserId } });

   let transaction = await Transaction.findAll({
      where: {
         WalletId: wallet.WalletId
      }
   });

   res.status(200).json(transaction);
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
   getTransactionHistoryByWalletId,
   getTransactionHistoryByUserId,
   getAllTransactionHistory,
};
