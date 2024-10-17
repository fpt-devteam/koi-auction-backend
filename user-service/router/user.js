const express = require("express");
const router = express.Router();
const { authenticate, verifyRole } = require("../middlewares/auth");
const controller = require("../controllers/user");

const adminOnly = verifyRole(["Admin"]);
const highRole = verifyRole(["Admin", "Staff"]);
const breederRole = verifyRole(["Breeder"]);

router.get("/profile", authenticate, controller.profile);
router.post("/register", controller.register);
router.post("/login", controller.login);
router.post("/logout", authenticate, controller.logout);
router.post("/forgot-password", controller.forgotPassword);
router.patch("/reset-password/:token", controller.resetPassword);
router.patch("/update-password", authenticate, controller.updatePassword);
router.patch("/update-profile", authenticate, controller.updateProfile);
router.delete("/delete", authenticate, controller.deleteAccount);

router.get("/breeder/profile", authenticate, breederRole, controller.getBreederProfile);

router.get("/manage/profile", authenticate, highRole, controller.getAllProfiles);
router.post("/manage/profile", authenticate, adminOnly, controller.manageCreateProfile);
router.get("/manage/profile/:id", authenticate, highRole, controller.getProfileById);
router.patch("/manage/profile/:id", authenticate, highRole, controller.manageUpdateProfile);
router.delete("/manage/profile/:id", authenticate, highRole, controller.manageDeleteProfile);

router.get("/manage/breeder/profile", controller.getAllBreederProfiles);
router.get("/manage/breeder/profile/:id", controller.getBreederProfileById);
router.patch("/manage/breeder/profile/:id", authenticate, highRole, controller.manageUpdateBreederProfile);
router.delete("/manage/breeder/profile/:id", authenticate, highRole, controller.manageDeleteBreederProfile);

module.exports = router;
