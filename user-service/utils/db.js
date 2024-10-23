const { Sequelize } = require("sequelize");

const sequelize = new Sequelize("KoiAuctionDB", "fptdevteam", "12345Fpt", {
  host: "fpt-koi.database.windows.net",
  dialect: "mssql",
  port: 1433,
  dialectOptions: {
    options: {
      encrypt: true,
      trustServerCertificate: false,
      enableArithAbort: true,
    },
  },
  pool: {
    max: 5,
    min: 0,
    acquire: 30000,
    idle: 10000,
  },
});

sequelize
  .authenticate()
  .then(() => {
    console.log("Connection has been established successfully.");
  })
  .catch((err) => {
    console.error("Unable to connect to the database:", err);
  });

module.exports = sequelize;
