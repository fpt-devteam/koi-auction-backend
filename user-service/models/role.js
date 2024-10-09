const { DataTypes } = require('sequelize');
const sequelize = require('../utils/db'); 

const Role = sequelize.define('Role', {
   RoleId: {
      type: DataTypes.INTEGER,
      primaryKey: true,
      autoIncrement: true
   },
   RoleName: {
      type: DataTypes.STRING,
      allowNull: false
   },
   CreatedAt: {
      type: DataTypes.DATE
   },
   UpdatedAt: {
      type: DataTypes.DATE
   }
}, {
   tableName: 'Role',
   timestamps: false
});

Role.sync({ force: false }).then(() => {
   console.log('Role table created');
});

module.exports = Role;