const router = require("express").Router();
const { authenticate, verifyRole } = require("../middlewares/auth");
const controller = require("../controller");
const User = require("../models/user");

const adminOnly = verifyRole(["Admin"]);
const highRole = verifyRole(["Admin", "Staff"]);
const breederRole = verifyRole(["Breeder"]);
const internalRole = verifyRole(["Internal"]);
const externalRole = verifyRole(["Admin", "Staff", "Breeder", "Member"]);
const breederAndAdminRole = verifyRole(["Admin", "Breeder"]);


router.patch('/manage/withdraw/:UserId/:Id', authenticate, highRole, controller.updateUserWithdrawStatusById);

// Role-based access control for Staff/Admin
router.get("/manage/get-wallet-balance", authenticate, highRole, controller.getAllWalletBalance);
router.get("/manage/get-wallet-balance/:UserId", authenticate, highRole, controller.getWalletBalanceByUserId);
router.get("/manage/get-transaction-history", authenticate, highRole, controller.getAllTransactionHistory);
router.get("/manage/get-transaction-history/:UserId", authenticate, highRole, controller.getTransactionHistoryByUserId);
router.post("/manage/refund", authenticate, highRole, controller.refund);

// Role-based access control for Internal Service
router.get("/internal/get-wallet-balance", authenticate, internalRole, controller.getAllWalletBalance);
router.get("/internal/get-wallet-balance/:UserId", authenticate, internalRole, controller.getWalletBalanceByUserId);
router.get("/internal/get-transaction-history", authenticate, internalRole, controller.getAllTransactionHistory);
router.get("/internal/get-transaction-history/:UserId", authenticate, internalRole, controller.getTransactionHistoryByUserId);

router.post('/internal/payment', authenticate, internalRole, controller.internalPayment);
router.post('/internal/refund-many', authenticate, internalRole, controller.internalRefundMany);

router.get("/statistics/transaction-history", authenticate, adminOnly, controller.getStatisticsTransactionHistory);
router.get("/breeder/statistics/transaction-history", authenticate, breederRole, controller.getBreederStatisticsTransactionHistory);
router.get("/breeder/statistics/get-sum-of-payout", controller.getSumOfPayoutOfBreeder);
router.get("/admin/statistics/get-sum-of-success-trans-by-type",authenticate, adminOnly, controller.getSumOfSuccessTransactionByTransTypeId);

router.get("/get-wallet-balance", authenticate, externalRole, controller.getWalletBalance);
router.get("/get-transaction-history", authenticate, externalRole, controller.getTransactionHistory);

router.post("/payment", controller.payment);
router.post("/deposit", authenticate, externalRole, controller.deposit);
router.post("/callback", controller.callback);
router.post("/withdraw", authenticate, externalRole, controller.withdraw);
router.post('/payout', authenticate, controller.payout);

module.exports = router;
