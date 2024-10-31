const { DataTypes } = require('sequelize');
const sequelize = require('../utils/db');

const Province = sequelize.define('Province', {
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
      type: DataTypes.STRING,
      allowNull: false
   },
   full_name_en: {
      type: DataTypes.STRING
   },
   code_name: {
      type: DataTypes.STRING
   },
   administrative_unit_id: {
      type: DataTypes.INTEGER
   },
   administrative_region_id: {
      type: DataTypes.INTEGER
   }
}, {
   tableName: 'provinces',
   timestamps: false
});

Province.sync({ force: false }).then(() => {
   console.log('Province table created');
});

module.exports = Province;