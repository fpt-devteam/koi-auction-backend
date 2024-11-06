const router = require("express").Router();
const { authenticate, verifyRole } = require("../middlewares/auth");
const controller = require("../controller");

const adminOnly = verifyRole(["Admin"]);
const highRole = verifyRole(["Admin", "Staff"]);
const breederRole = verifyRole(["Breeder"]);

router.get("/get-wallet-balance", authenticate, controller.getWalletBalance);
router.get("/get-transaction-history", authenticate, controller.getTransactionHistory);

router.post("/payment", controller.payment);
router.post("/deposit", authenticate, controller.deposit);
router.post("/callback", controller.callback);
router.post("/withdraw", authenticate, controller.withdraw);

router.patch('/manage/withdraw/:UserId/:Id', authenticate, adminOnly, controller.updateUserWithdrawStatusById);

// Role-based access control for Internal Service
router.get("/internal/get-wallet-balance", controller.getAllWalletBalance);
router.get("/internal/get-wallet-balance/:UserId", controller.getWalletBalanceByUserId);
router.get("/internal/get-transaction-history", controller.getAllTransactionHistory);
router.get("/internal/get-transaction-history/:UserId", controller.getTransactionHistoryByUserId);

router.post('/internal/payment/:UserId', controller.internalPayment);

module.exports = router;
