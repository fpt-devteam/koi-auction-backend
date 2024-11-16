const { DataTypes } = require('sequelize');
const sequelize = require('../utils/db'); 
const Role = require('./role');
const Province = require('./provinces');
const District = require('./districts');
const Ward = require('./wards');

const User = sequelize.define('User', {
   UserId: {
      type: DataTypes.INTEGER,
      primaryKey: true,
      autoIncrement: true
   },
   Username: {
      type: DataTypes.STRING,
      allowNull: false
   },
   Password: {
      type: DataTypes.STRING
   },
   Phone: {
      type: DataTypes.STRING
   },
   Email: {
      type: DataTypes.STRING,
      allowNull: false
   },
   Active: {
      type: DataTypes.BOOLEAN,
      allowNull: false
   },
   UserRoleId: {
      type: DataTypes.INTEGER,
      references: {
         model: Role,
         key: 'RoleId'
      }
   },
   FirstName: {
      type: DataTypes.STRING,
   },
   LastName: {
      type: DataTypes.STRING,
   },
   GoogleId: {
      type: DataTypes.STRING
   },
   ProvinceCode: {
      type: DataTypes.STRING
   },
   DistrictCode: {
      type: DataTypes.STRING
   },
   WardCode: {
      type: DataTypes.STRING
   },
   Address: {
      type: DataTypes.STRING
   },
   Verified: {
      type: DataTypes.INTEGER
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
   tableName: 'User',
   timestamps: false
});

User.sync({ force: false }).then(() => {
   console.log('User table created');
}).catch(err => {
   console.log(err);
});

module.exports = User;
