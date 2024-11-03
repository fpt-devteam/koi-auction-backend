const { DataTypes } = require('sequelize');
const sequelize = require('../utils/db');
const Province = require('./provinces');

// CREATE TABLE districts (
// 	code nvarchar(20) NOT NULL,
// 	name nvarchar(255) NOT NULL,
// 	name_en nvarchar(255) NULL,
// 	full_name nvarchar(255) NULL,
// 	full_name_en nvarchar(255) NULL,
// 	code_name nvarchar(255) NULL,
// 	province_code nvarchar(20) NULL,
// 	administrative_unit_id integer NULL,
// 	CONSTRAINT districts_pkey PRIMARY KEY (code)
// );


const District = sequelize.define('District', {
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
   province_code: {
      type: DataTypes.STRING,
      references: {
         model: Province,
         key: 'code'
      }
   },
   administrative_unit_id: {
      type: DataTypes.INTEGER
   }
}, {
   tableName: 'districts',
   timestamps: false
});

District.sync({ force: false }).then(() => {
   console.log('District table created');
});

module.exports = District;