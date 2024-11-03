const { DataTypes } = require('sequelize');
const sequelize = require('../utils/db');
const Wallet = require('./wallet');
const TransactionType = require('./transaction-type');
const TransactionStatus = require('./transaction-status');

const Transaction = sequelize.define('Transaction', {
   TransId: {
      type: DataTypes.INTEGER,
      primaryKey: true,
      autoIncrement: true
   },
   WalletId: {
      type: DataTypes.INTEGER,
      references: {
         model: Wallet,
         key: 'WalletId'
      }
   },
   Amount: {
      type: DataTypes.DECIMAL(10, 2),
      defaultValue: 0,
   },
   TransTypeId: {
      type: DataTypes.INTEGER,
      references: {
         model: TransactionType,
         key: 'TransTypeId'
      }
   },
   StatusId: {
      type: DataTypes.INTEGER,
      references: {
         model: TransactionStatus,
         key: 'TransStatusId'
      }
   },
   AppTransId: {
      type: DataTypes.STRING
   },
   SoldLotId: {
      type: DataTypes.INTEGER
   },
   BalanceAfter: {
      type: DataTypes.DECIMAL(10, 2)
   },
   Description: {
      type: DataTypes.STRING
   },
   CreatedAt: {
      type: DataTypes.DATE,
      defaultValue: DataTypes.NOW
   }
}, {
   tableName: 'Transaction',
   timestamps: false
});

Transaction.sync({ force: false }).then(() => {
   console.log('Transaction table created');
}).catch((err) => {
   console.log(err);
});

module.exports = Transaction;