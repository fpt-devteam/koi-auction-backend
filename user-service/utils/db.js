const { Sequelize } = require("sequelize");

const sequelize = new Sequelize("KoiAuctionDB", "sqlserver", "sa123456!", {
  host: "35.247.172.255",
  dialect: "mssql",
  port: 1433,
  dialectOptions: {
    options: {
      encrypt: true,
      trustServerCertificate: true,
      enableArithAbort: true,
    },
    ssl: {
      rejectUnauthorized: false, // Bỏ qua xác thực chứng chỉ
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
