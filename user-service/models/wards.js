const { DataTypes } = require('sequelize');
const sequelize = require('../utils/db');
const District = require('./districts');

const Ward = sequelize.define('Ward', {
   code: {
      type: DataTypes.STRING,
      primaryKey: true
   },
   name: {
      type: DataTypes.STRING,
      allowNull: false
   },
   name_en: {
      type: DataTypes.STRING
   },
   full_name: {
      type: DataTypes.STRING
   },
   full_name_en: {
      type: DataTypes.STRING
   },
   code_name: {
      type: DataTypes.STRING
   },
   district_code: {
      type: DataTypes.STRING,
      references: {
         model: District,
         key: 'code'
      }
   },
   administrative_unit_id: {
      type: DataTypes.INTEGER
   }
}, {
   tableName: 'wards',
   timestamps: false
});

Ward.sync({ force: false }).then(() => {
   console.log('Ward table created');
});

module.exports = Ward;