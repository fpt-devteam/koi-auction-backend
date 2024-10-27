const { DataTypes } = require('sequelize');
const sequelize = require('../utils/db'); 
const User = require('./user');

const Wallet = sequelize.define('Wallet', {
   WalletId: {
      type: DataTypes.INTEGER,
      primaryKey: true,
      autoIncrement: true
   },
   UserId: {
      type: DataTypes.INTEGER,
      references: {
         model: User,
         key: 'UserId'
      }
   },
   Balance: {
      type: DataTypes.DECIMAL(10, 2)
   },
   Currency: {
      type: DataTypes.STRING(5),
   },
   CreatedAt: {
      type: DataTypes.DATE,
      defaultValue: DataTypes.NOW
   },
   UpdatedAt: {
      type: DataTypes.DATE,
      defaultValue: DataTypes.NOW
   }
}, {
   tableName: 'Wallet',
   timestamps: false
});

Wallet.sync({ force: false }).then(() => {
   console.log('Wallet table created');
}).catch((err) => {
   console.log(err);
});

module.exports = Wallet;