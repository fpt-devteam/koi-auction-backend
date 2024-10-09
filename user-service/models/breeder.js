const { DataTypes } = require('sequelize');
const sequelize = require('../utils/db'); 
const User = require('./user');

const BreederDetail = sequelize.define('BreederDetail', {
   BreederId: {
      type: DataTypes.INTEGER,
      primaryKey: true,
      references: {
         model: User,
         key: 'UserId'
      }
   },

   FarmName: {
      type: DataTypes.STRING
   },
   Certificate: {
      type: DataTypes.STRING
   },
   About: {
      type: DataTypes.STRING
   }
}, {
   tableName: 'BreederDetail',
   timestamps: false
});

BreederDetail.sync({ force: false }).then(() => {
   console.log('BreederDetail table created');
});

module.exports = BreederDetail;