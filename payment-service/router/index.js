const router = require('express').Router();
const { authenticate } = require('../middlewares/auth');
const controller = require('../controller');

router.get('/get-wallet-balance', authenticate, controller.getWalletBalance);
router.get('/get-transaction-history', authenticate, controller.getTransactionHistory);

router.post('/payment', authenticate, controller.payment);
router.post('/deposit', authenticate, controller.deposit);
router.post('/callback', controller.callback);
router.post('/reload-wallet', authenticate, controller.reloadWallet);

module.exports = router;