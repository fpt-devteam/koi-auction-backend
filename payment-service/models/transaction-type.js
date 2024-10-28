const { DataTypes } = require('sequelize');
const sequelize = require('../utils/db');

const TransactionType = sequelize.define('TransactionType', {
   TransTypeId: {
      type: DataTypes.INTEGER,
      primaryKey: true,
      autoIncrement: true
   },
   TransTypeName: {
      type: DataTypes.STRING,
      allowNull: false,
      unique: true
   },
   TransTypeDescription: {
      type: DataTypes.STRING
   },
   CreatedAt: {
      type: DataTypes.DATE,
      defaultValue: DataTypes.NOW
   }
}, {
   tableName: 'TransactionType',
   timestamps: false
});

TransactionType.sync({ force: false }).then(() => {
   console.log('TransactionType table created');
}).catch((err) => {
   console.log(err);
});

module.exports = TransactionType;