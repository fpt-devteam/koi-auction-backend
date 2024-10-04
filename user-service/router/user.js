const express = require('express');
const router = express.Router();
const { authenticate } = require('../middlewares/auth');
const controller = require('../controller/user');


router.get('/profile', authenticate, controller.profile);
router.post('/register', controller.register);
router.post('/login', controller.login);
router.post('/logout', authenticate, controller.logout);
router.patch('/update-password', authenticate, controller.updatePassword);
router.patch('/update-profile', authenticate, controller.updateProfile);
router.delete('/delete', authenticate, controller.deleteAccount);

// router.get('/all', authenticate, controller.all);
// router.get('/all/:id', controller.allById);
// router.get('/all/:id/:name', controller.allByIdAndName);

module.exports = router;