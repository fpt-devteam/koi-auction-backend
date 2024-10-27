const { DataTypes } = require('sequelize');
const sequelize = require('../utils/db');

const TransactionStatus = sequelize.define('TransactionStatus', {
   TransStatusId: {
      type: DataTypes.INTEGER,
      primaryKey: true,
      autoIncrement: true,
   },
   TransStatusName: {
      type: DataTypes.STRING,
      allowNull: false,
      unique: true,
   },
   TransStatusDescription: {
      type: DataTypes.STRING,
   },
   CreatedAt: {
      type: DataTypes.DATE,
      defaultValue: DataTypes.NOW,
   },
}, {
   tableName: 'TransactionStatus',
   timestamps: false,
});

TransactionStatus.sync({ force: false }).then(() => {
   console.log('TransactionStatus table created');
}).catch((err) => {
   console.log(err);
});

module.exports = TransactionStatus;