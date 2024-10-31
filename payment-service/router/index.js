const router = require("express").Router();
const { authenticate, verifyRole } = require("../middlewares/auth");
const controller = require("../controller");

router.get("/get-wallet-balance", authenticate, controller.getWalletBalance);
router.get(
  "/get-transaction-history",
  authenticate,
  controller.getTransactionHistory
);

router.post("/payment", controller.payment);
router.post("/deposit", authenticate, controller.deposit);
router.post("/callback", controller.callback);
router.post("/reload-wallet", authenticate, controller.reloadWallet);

// Role-based access control for Internal Service
router.get("/internal/get-wallet-balance", authenticate, verifyRole(["Internal Service"]), controller.getAllWalletBalance);
router.get("/internal/get-wallet-balance/:UserId", controller.getWalletBalanceByUserId);
router.get("/internal/get-transaction-history", authenticate, verifyRole(["Internal Service"]),controller.getAllTransactionHistory);
router.get("/internal/get-transaction-history/:UserId", authenticate, verifyRole(["Internal Service"]),controller.getTransactionHistoryByUserId);

router.post('/internal/payment/:UserId', authenticate, verifyRole(['Internal Service']), controller.internalPayment);

module.exports = router;
