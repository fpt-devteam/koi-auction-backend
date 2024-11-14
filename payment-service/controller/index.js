const axios = require('axios'); // npm install axios
const CryptoJS = require('crypto-js'); // npm install crypto-js
const moment = require('moment'); // npm install moment
const qs = require('qs'); // npm install qs
const { Op, NUMBER } = require('sequelize');
const User = require('../models/user');
const Transaction = require('../models/transaction');
const Wallet = require('../models/wallet');
const TransactionType = require('../models/transaction-type');
const TransactionStatus = require('../models/transaction-status');
const BreederDetail = require('../../user-service/models/breeder');

const PAYOUT = 4;
const WITHDRAW = 1;
const SUCCESS = 2;

const deposit = async (req, res) => {
   const { Amount } = req.body;
   const { UserId } = req.user;

   if (!Amount) return res.status(400).json({ message: "Amount is required" });
   if (isNaN(Amount)) return res.status(400).json({ message: "Amount must be a number" });
   if (Amount <= 0) return res.status(400).json({ message: "Amount must be greater than 0" });

   const wallet = await Wallet.findOne({ where: { UserId: UserId } });

   await Transaction.update(
      { StatusId: 3 },
      { where: { WalletId: wallet.WalletId, StatusId: 1 } }
   );

   const config = {
      app_id: "2553",
      key1: "PcY4iZIKFCIdgZvA6ueMcMHHUbRLYjPL",
      key2: "kLtgPl8HHhfvMuDHPwKfgfsY4Ydm9eIz",
      endpoint: "https://sb-openapi.zalopay.vn/v2/create",
   };

   const embed_data = {
      redirecturl: "localhost:5173",
   };
   const items = [{}];
   const transID = Math.floor(Math.random() * 1000000);
   const app_trans_id = `${moment().format("YYMMDD")}_${transID}`;
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
      callback_url:
         "https://4514-118-71-221-28.ngrok-free.app/payment-service/callback",
   };

   console.log(`app_trans_id = ${app_trans_id}`);

   const data =
      config.app_id +
      "|" +
      order.app_trans_id +
      "|" +
      order.app_user +
      "|" +
      order.amount +
      "|" +
      order.app_time +
      "|" +
      order.embed_data +
      "|" +
      order.item;
   order.mac = CryptoJS.HmacSHA256(data, config.key1).toString();

   const response = await axios.post(config.endpoint, null, { params: order });

   try {
      await Transaction.create({
         UserId: UserId,
         Amount: Amount,
         WalletId: wallet.WalletId,
         StatusId: 1,
         TransTypeId: 3,
         AppTransId: app_trans_id,
         BalanceBefore: wallet.Balance,
         Description: "Nạp tiền vào ví",
         CreatedAt: Date.now(),
      });
   } catch (err) {
      console.log(err);
   }

   res.status(200).json(response.data);
};

const callback = async (req, res) => {
   const config = {
      key2: "kLtgPl8HHhfvMuDHPwKfgfsY4Ydm9eIz",
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
      } else {
         // thanh toán thành công
         // merchant cập nhật trạng thái cho đơn hàng
         let dataJson = JSON.parse(dataStr, config.key2);
         console.log(
            "update order's status = success where app_trans_id =",
            dataJson["app_trans_id"]
         );

         result.return_code = 1;
         result.return_message = "success";

         const appTransId = dataJson["app_trans_id"];

         const transaction = await Transaction.findOne({
            where: { AppTransId: appTransId },
         });
         if (transaction) {
            try {
               await Transaction.update(
                  { StatusId: 2 },
                  { where: { AppTransId: appTransId } }
               );
               await Wallet.update(
                  { Balance: transaction.BalanceBefore + transaction.Amount },
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
      endpoint: "https://sb-openapi.zalopay.vn/v2/query",
   };

   let postData = {
      app_id: config.app_id,
      app_trans_id: orderId, // Input your app_trans_id
   };

   let data = postData.app_id + "|" + postData.app_trans_id + "|" + config.key1; // appid|app_trans_id|key1
   postData.mac = CryptoJS.HmacSHA256(data, config.key1).toString();

   const response = await axios.post(config.endpoint, null, {
      params: postData,
   });
   return response.data;
};

const refreshWallet = async (UserId) => {
   const wallet = await Wallet.findOne({ where: { UserId: UserId } });

   const transaction = await Transaction.findAll({
      where: {
         WalletId: wallet.WalletId,
         StatusId: 1,
         TransTypeId: { [Op.ne]: 1 }
      }
   });

   let balance = Number(wallet.Balance);

   for (const trans of transaction) {
      try {
         const status = await checkOrderStatus(trans.AppTransId);
         if (status.return_code == 1) {
            balance += trans.Amount;
            await Transaction.update(
               { StatusId: 2 },
               { where: { AppTransId: trans.AppTransId } }
            );
         } else if (status.return_code == 2) {
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

const getWalletBalance = async (req, res) => {
   const { UserId } = req.user;

   const refresh = await refreshWallet(UserId);
   if (refresh.status == 500) {
      return res.status(500).json({ message: "Internal Server Error" });
   }

   // try {
   //    await axios.post("http://localhost:3002/api/bid/update-balance", {
   //       Amount: refresh.Balance,
   //       BidderId: UserId,
   //    });
   // } catch (err) {
   //    console.log(err);
   // }

   res.status(200).json({ balance: refresh.Balance });
};

const getTransactionHistory = async (req, res) => {
   const { UserId } = req.user;
   const { Status, TransType } = req.body;

   try {
      const wallet = await Wallet.findOne({ where: { UserId: UserId } });
      const [transactionTypes, transactionStatus] = await Promise.all([
         TransactionType.findAll(),
         TransactionStatus.findAll()
      ]);

      const TransTypeId = transactionTypes.find((type) => type.TransTypeName == TransType)?.TransTypeId;
      const StatusId = transactionStatus.find((status) => status.TransStatusName == Status)?.TransStatusId;

      let transactions = await Transaction.findAll({
         where: {
            WalletId: wallet.WalletId,
            TransTypeId: TransTypeId || { [Op.ne]: null },
            StatusId: StatusId || { [Op.ne]: null }
         }
      });

      let result = [];
      await Promise.all(transactions.map(async (transaction) => {
         const wallet = await Wallet.findByPk(transaction.WalletId);
         result.push({
            TransId: transaction.TransId,
            UserId: wallet.UserId,
            Amount: transaction.Amount,
            WalletId: transaction.WalletId,
            Status: transactionStatus.find((status) => status.TransStatusId == transaction.StatusId).TransStatusName,
            TransType: transactionTypes.find((type) => type.TransTypeId == transaction.TransTypeId).TransTypeName,
            BalanceBefore: transaction.BalanceBefore,
            BalanceAfter: transaction.BalanceAfter,
            Description: transaction.Description,
         });
      }));

      res.status(200).json(result);
   } catch (err) {
      console.log(err);
      return res.status(500).json({ message: "Internal Server Error" });
   }
}

const payment = async (req, res) => {
   const { Amount, SoldLotId } = req.body;
   const { UserId } = req.user;

   if (!Amount) return res.status(400).json({ message: "Amount is required" });
   if (isNaN(Amount))
      return res.status(400).json({ message: "Amount must be a number" });
   if (Amount <= 0)
      return res.status(400).json({ message: "Amount must be greater than 0" });

   const wallet = await Wallet.findOne({ where: { UserId: UserId } });

   if (wallet.Balance < Amount)
      return res.status(400).json({ message: "Not enough money" });

   try {
      await Transaction.create({
         UserId: UserId,
         Amount: Amount,
         WalletId: wallet.WalletId,
         StatusId: 2,
         TransTypeId: 2,
         SoldLotId: SoldLotId,
         BalanceBefore: wallet.Balance,
         Description: "Thanh toán hóa đơn",
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
   await Promise.all(
      wallets.map(async (wallet) => {
         const refresh = await refreshWallet(wallet.UserId);
         wallet.Balance = refresh.Balance;
      })
   );

   res.status(200).json(wallets);
};

const getWalletBalanceByUserId = async (req, res) => {
   const { UserId } = req.params;
   console.log("UserId = ", UserId);
   try {
      // const wallet = await Wallet.findOne({ where: { UserId: UserId } });
      const refresh = await refreshWallet(UserId);

      res.status(200).json({
         UserId: Number(UserId),
         BidderId: Number(UserId),
         WalletId: refresh.WalletId,
         Balance: refresh.Balance,
         Currency: refresh.Currency,
      });
   } catch (err) {
      console.log(err);
      return res.status(500).json({ message: "Internal Server Error" });
   }
};

const getAllTransactionHistory = async (req, res) => {
   const { Status, TransType } = req.body;

   const transactionTypes = await TransactionType.findAll();
   const transactionStatus = await TransactionStatus.findAll();

   const TransTypeId = transactionTypes.find((type) => type.TransTypeName == TransType)?.TransTypeId;
   const StatusId = transactionStatus.find((status) => status.TransStatusName == Status)?.TransStatusId;

   let transactions = await Transaction.findAll({
      where: {
         TransTypeId: TransTypeId || { [Op.ne]: null },
         StatusId: StatusId || { [Op.ne]: null }
      }
   });

   let result = [];
   await Promise.all(transactions.map(async (transaction) => {
      const wallet = await Wallet.findByPk(transaction.WalletId);
      result.push({
         TransId: transaction.TransId,
         UserId: wallet.UserId,
         Amount: transaction.Amount,
         WalletId: transaction.WalletId,
         Status: transactionStatus.find((status) => status.TransStatusId == transaction.StatusId).TransStatusName,
         TransType: transactionTypes.find((type) => type.TransTypeId == transaction.TransTypeId).TransTypeName,
         BalanceBefore: transaction.BalanceBefore,
         BalanceAfter: transaction.BalanceAfter,
         Description: transaction.Description,
      });
   }));

   res.status(200).json(result);
}

const getTransactionHistoryByUserId = async (req, res) => {
   const { UserId } = req.params;
   const { Status, TransType } = req.body;

   try {
      const wallet = await Wallet.findOne({ where: { UserId: UserId } });

      if (!wallet) return res.status(404).json({ message: "User not found" });

      const [transactionTypes, transactionStatus] = await Promise.all([
         TransactionType.findAll(),
         TransactionStatus.findAll()
      ]);

      const TransTypeId = transactionTypes.find((type) => type.TransTypeName == TransType)?.TransTypeId;
      const StatusId = transactionStatus.find((status) => status.TransStatusName == Status)?.TransStatusId;

      let transactions = await Transaction.findAll({
         where: {
            WalletId: wallet.WalletId,
            TransTypeId: TransTypeId || { [Op.ne]: null },
            StatusId: StatusId || { [Op.ne]: null }
         }
      });

      let result = [];
      await Promise.all(transactions.map(async (transaction) => {
         const wallet = await Wallet.findByPk(transaction.WalletId);
         result.push({
            TransId: transaction.TransId,
            UserId: wallet.UserId,
            Amount: transaction.Amount,
            WalletId: transaction.WalletId,
            Status: transactionStatus.find((status) => status.TransStatusId == transaction.StatusId).TransStatusName,
            TransType: transactionTypes.find((type) => type.TransTypeId == transaction.TransTypeId).TransTypeName,
            BalanceBefore: transaction.BalanceBefore,
            Description: transaction.Description,
         });
      }));
      res.status(200).json(result);
   } catch (err) {
      console.log(err);
      return res.status(500).json({ message: "Internal Server Error" });
   }
}

const internalPayment = async (req, res) => {
   console.log(`req.body = ${JSON.stringify(req.body)}`);
   const { userId, soldLotId, amount, description } = req.body;
   const Amount = Number(amount);
   const UserId = Number(userId);
   console.log("amount = ", amount);
   console.warn("UserId = ", UserId);
   console.warn("Amount = ", Amount);
   if (!UserId) return res.status(400).json({ message: "UserId is required" });
   if (!Amount) return res.status(400).json({ message: "Amount is required" });
   if (isNaN(UserId))
      return res.status(400).json({ message: "UserId must be a number" });
   if (isNaN(Amount))
      return res.status(400).json({ message: "Amount must be a number" });
   if (Amount <= 0)
      return res.status(400).json({ message: "Amount must be greater than 0" });

   const wallet = await Wallet.findOne({ where: { UserId: UserId } });
   if (wallet.Balance < Amount)
      return res.status(400).json({ message: "Not enough money" });

   try {

      await Transaction.create({
         UserId: UserId,
         Amount: Amount,
         WalletId: wallet.WalletId,
         StatusId: 2,
         TransTypeId: 2,
         BalanceBefore: wallet.Balance,
         SoldLotId: soldLotId,
         Description: description,
         CreatedAt: Date.now(),
      });

      await Wallet.update(
         { Balance: wallet.Balance - Amount },
         { where: { WalletId: wallet.WalletId } }
      );

      return res.status(200).json({ message: "Payment successfully" });
   } catch (err) {
      console.log(err);
      return res.status(500).json({ message: "Internal Server Error" });
   }
}

const withdraw = async (req, res) => {
   const { Amount, BankAccount, BankName, AccountHolder } = req.body;
   const { UserId } = req.user;

   if (!Amount) return res.status(400).json({ message: "Amount is required" });
   if (isNaN(Amount)) return res.status(400).json({ message: "Amount must be a number" });
   if (Amount <= 0) return res.status(400).json({ message: "Amount must be greater than 0" });

   const wallet = await Wallet.findOne({ where: { UserId: UserId } });

   if (wallet.Balance < Amount) return res.status(400).json({ message: "Not enough money" });

   try {
      await Transaction.create({
         UserId: UserId,
         Amount: Amount,
         WalletId: wallet.WalletId,
         StatusId: 1,
         TransTypeId: 1,
         BalanceBefore: wallet.Balance,
         Description: `Bank Account: ${BankAccount}, Bank Name: ${BankName}, Account Holder: ${AccountHolder}`,
         CreatedAt: Date.now(),
      });

   } catch (err) {
      console.log(err);
   }

   res.status(200).json({ message: "Withdraw post successfully" });
};

const payout = async (req, res) => {
   const { BreederId, Amount } = req.body;

   if (!BreederId) return res.status(400).json({ message: "BreederId is required" });
   if (!Amount) return res.status(400).json({ message: "Amount is required" });
   if (isNaN(Amount)) return res.status(400).json({ message: "Amount must be a number" });
   if (Amount <= 0) return res.status(400).json({ message: "Amount must be greater than 0" });

   const breeder = await BreederDetail.findByPk(BreederId);
   if (!breeder) return res.status(404).json({ message: "Breeder not found" });

   const wallet = await Wallet.findOne({ where: { UserId: BreederId } });
   if (!wallet) return res.status(404).json({ message: "Breeder wallet not found" });

   const transferAmount = Amount * 0.9;

   try {
      await Promise.all([
         await Transaction.create({
            UserId: BreederId,
            Amount: transferAmount,
            WalletId: wallet.WalletId,
            StatusId: 2,
            TransTypeId: 4,
            BalanceBefore: wallet.Balance,
            Description: "Thanh toán hóa đơn",
            CreatedAt: Date.now(),
         }),
         await Wallet.update(
            { Balance: wallet.Balance + transferAmount },
            { where: { WalletId: wallet.WalletId } }
         )
      ]);

   } catch (err) {
      console.log(err);
      return res.status(500).json({ message: "Internal Server Error" });
   }

   res.status(200).json({ message: "Payout successfully" });
}

const updateUserWithdrawStatusById = async (req, res) => {
   const { UserId, Id } = req.params;
   const { Status } = req.body;

   if (!UserId) return res.status(400).json({ message: "UserId is required" });
   if (!Id) return res.status(400).json({ message: "Id is required" });
   if (!Status) return res.status(400).json({ message: "Status is required" });

   try {
      const wallet = await Wallet.findOne({ where: { UserId: UserId } });

      if (!wallet) return res.status(404).json({ message: "User not found" });

      const transaction = await Transaction.findOne({ where: { WalletId: wallet.WalletId, TransId: Id } });
      if (!transaction) return res.status(404).json({ message: "Transaction not found" });
      if (transaction.TransTypeId != 1) return res.status(400).json({ message: "Transaction is not withdraw" });
      if (transaction.StatusId != 1) return res.status(400).json({ message: "Transaction is not pending" });

      if (wallet.Balance < transaction.Amount) return res.status(400).json({ message: "Not enough money" });

      const transactionStatus = await TransactionStatus.findAll();
      const StatusId = transactionStatus.find((status) => status.TransStatusName == Status)?.TransStatusId;

      await Wallet.update(
         { Balance: (StatusId == 2) ? wallet.Balance - transaction.Amount : wallet.Balance },
         { where: { WalletId: wallet.WalletId } }
      );

      await Transaction.update(
         { StatusId: StatusId, BalanceBefore: wallet.Balance },
         { where: { WalletId: wallet.WalletId, TransTypeId: 1, TransId: Id } }
      );

      res.status(200).json({ message: "Update withdraw status successfully" });
   } catch (err) {
      console.log(err);
      return res.status(500).json({ message: "Internal Server Error" });
   }
}

const getStatisticsTransactionHistory = async (req, res) => {
   const { Status, TransType } = req.body;

   const transactionTypes = await TransactionType.findAll();
   const transactionStatus = await TransactionStatus.findAll();

   const TransTypeId = transactionTypes.find((type) => type.TransTypeName == TransType)?.TransTypeId;
   const StatusId = transactionStatus.find((status) => status.TransStatusName == Status)?.TransStatusId;

   let transactions = await Transaction.findAll({
      where: {
         TransTypeId: TransTypeId || { [Op.ne]: null },
         StatusId: StatusId || { [Op.ne]: null }
      }
   });

   let { start, end, dayAmount, userId } = req.query;
   let totalAmount = 0;
   await Promise.all(transactions.map(async (transaction) => {
      const wallet = await Wallet.findByPk(transaction.WalletId);
      let resultFlag = true;
      if (userId) {
         if (wallet.UserId != userId) {
            resultFlag = false;
         }
      }
      if (start && end) {
         const date = moment(transaction.CreatedAt).format("YYYY-MM-DD");
         start = moment(start).format("YYYY-MM-DD");
         end = moment(end).format("YYYY-MM-DD");
         if (date < start || date > end) {
            resultFlag = false;
         }
      } 
      if (dayAmount) {
         const date = moment(transaction.CreatedAt).format("YYYY-MM-DD");
         const dateBefore = moment().subtract(dayAmount, "days").format("YYYY-MM-DD");
         if (date < dateBefore) {
            resultFlag = false;
         }
      }

      totalAmount += (resultFlag) ? transaction.Amount : 0;
   }));

   res.status(200).json({ TotalAmount: totalAmount });
}

const getBreederStatisticsTransactionHistory = async (req, res) => {
   const { UserId } = req.user;
   const { Status, TransType } = req.body;
   let { dayAmount } = req.query;
   // return a list each day with total amount 
   const transactionTypes = await TransactionType.findAll();
   const transactionStatus = await TransactionStatus.findAll();

   const wallet = await Wallet.findOne({ where: { UserId: UserId } });
   let transactions = await Transaction.findAll({
      where: {
         WalletId: wallet.WalletId,
      },
   });
   transactions.map((transaction) => {
      transaction.TransType = transactionTypes.find((type) => type.TransTypeId == transaction.TransTypeId).TransTypeName;
      transaction.Status = transactionStatus.find((status) => status.TransStatusId == transaction.StatusId).TransStatusName;
   });
   transactions = transactions.filter((transaction) => {
      if (TransType && transaction.TransType != TransType) return false;
      if (Status && transaction.Status != Status) return false;
      const date = moment(transaction.CreatedAt).format("YYYY-MM-DD");
      const dateBefore = moment().subtract(dayAmount, "days").format("YYYY-MM-DD");
      return date >= dateBefore;
   });
   let result = [];
   transactions.forEach((transaction) => {
      const date = moment(transaction.CreatedAt).format("YYYY-MM-DD");
      const index = result.findIndex((item) => item.date == date);
      if (index == -1) {
         result.push({ date: date, totalAmount: transaction.Amount });
      } else {
         result[index].totalAmount += transaction.Amount;
      }
   });
   res.status(200).json(result);
}

//get sum of payout of breeder by breederId each day in dayAmount
//  SELECT * FROM [Transaction]
// WHERE [TransTypeId] = 4 AND StatusId = 2 

const getSumOfPayoutOfBreeder = async (req, res) => {
   const { userId } = req.query;
   let { dayAmount } = req.query;

   try {
      const wallet = await Wallet.findOne({ where: { UserId: userId } });

      // Lấy danh sách các giao dịch của breeder dựa trên userId, loại giao dịch và trạng thái
      let transactions = await Transaction.findAll({
         where: {
            WalletId: wallet.WalletId, // Sử dụng UserId thay vì WalletId
            TransTypeId: 4, // Loại giao dịch là payout
            StatusId: 2, // Trạng thái giao dịch
         },
         order: [['CreatedAt', 'DESC']], // Sắp xếp từ mới nhất đến cũ nhất
      });

      // Khởi tạo một mảng để lưu kết quả với kích thước dayAmount
      console.log("wallet", wallet);
      let result = [];
      for (let i = 0; i <= dayAmount; i++) {
         // Tính ngày cho từng phần tử
         const date = moment().subtract(i, 'days').format('YYYY-MM-DD');
         // Tính tổng payout cho ngày đó
         const totalAmount = transactions
            .filter((transaction) => moment(transaction.CreatedAt).format('YYYY-MM-DD') === date)
            .reduce((sum, transaction) => sum + transaction.Amount, 0);
         
         // Thêm đối tượng với ngày và tổng số tiền vào mảng kết quả
         const dateFormatted = moment(date).format('MMM DD');
         result.push({ dateFormatted, totalAmount });
      }

      res.status(200).json(result);
   } catch (error) {
      console.error("Error fetching payout data:", error);
      res.status(500).json({ message: "Internal Server Error" });
   }
};

const getSumOfSuccessTransactionByTransTypeId = async (req, res) => {
   let { dayAmount } = req.query;
   let { transTypeId } = req.query;

   try {

      // Lấy danh sách các giao dịch của breeder dựa trên userId, loại giao dịch và trạng thái
      let transactions = await Transaction.findAll({
         where: {
            TransTypeId: transTypeId, // Loại giao dịch là payout
            StatusId: 2, // Trạng thái giao dịch
         },
         order: [['CreatedAt', 'DESC']], // Sắp xếp từ mới nhất đến cũ nhất
      });

      // sum of all transaction with tran type and day amount
      let result = 0;
      let dayBegin = moment().subtract(dayAmount, 'days');
      transactions.forEach((transaction) => {
         if (moment(transaction.CreatedAt).isAfter(dayBegin)) {
            result += transaction.Amount;
         }
      });
      res.status(200).json({ totalAmount: result, transTypeId: transTypeId, dayAmount: dayAmount });
   } catch (error) {
      console.error("Error fetching payout data:", error);
      res.status(500).json({ message: "Internal Server Error" });
   }
}


module.exports = {
   deposit,
   payment,
   callback,
   getWalletBalance,
   getTransactionHistory,
   getAllWalletBalance,
   getWalletBalanceByUserId,
   getTransactionHistoryByUserId,
   getAllTransactionHistory,
   internalPayment,
   withdraw,
   payout,
   updateUserWithdrawStatusById,
   getStatisticsTransactionHistory,
   getBreederStatisticsTransactionHistory,
   getSumOfPayoutOfBreeder,
   getSumOfSuccessTransactionByTransTypeId
};
