const express = require("express");
const router = express.Router();
const { authenticate, verifyRole } = require("../middlewares/auth");
const controller = require("../controllers/user");
const passport = require("../utils/passport");

const adminOnly = verifyRole(["Admin"]);
const highRole = verifyRole(["Admin", "Staff"]);
const breederRole = verifyRole(["Breeder"]);

router.get("/profile", authenticate, controller.profile);
router.post("/register", controller.register);
router.post("/verify-email", controller.emailVerification);
router.post("/login", controller.login);
router.post("/logout", authenticate, controller.logout);
router.post("/forgot-password", controller.forgotPassword);
router.patch("/reset-password/:token", controller.resetPassword);
router.patch("/update-password", authenticate, controller.updatePassword);
router.patch("/update-profile", authenticate, controller.updateProfile);
router.delete("/delete", authenticate, controller.deleteAccount);

router.post("/auth/google", controller.googleAuth);

router.get("/address/province", controller.getProvinces);
router.get("/address/province/:code", controller.getProvinceById);
router.get("/address/district", controller.getDistricts);
router.get("/address/district/:provinceId", controller.getDistrictByProvinceId);
router.get("/address/ward", controller.getWards);
router.get("/address/ward/:districtId", controller.getWardByDistrictId);

router.get("/unverified-breeders", authenticate, highRole, controller.getUnverifiedBreeders);
router.patch("/verify-breeder/:UserId", authenticate, highRole, controller.verifyBreeder);

router.get("/breeder/profile", authenticate, breederRole, controller.getBreederProfile);

router.get("/manage/profile", controller.getAllProfiles); 
router.post("/manage/profile", controller.manageCreateProfile);
router.get("/manage/profile/:id", controller.getProfileById);
router.patch("/manage/profile/:id", authenticate, highRole, controller.manageUpdateProfile);
router.delete("/manage/profile/:id", authenticate, highRole, controller.manageDeleteProfile);
router.get("/manage/profile/address/:id", controller.getProfileAddressById);
router.get("/manage/detail-profile/:id", authenticate, highRole, controller.manageGetDetailProfile);

router.get("/manage/breeder/profile", controller.getAllBreederProfiles);
router.get("/manage/breeder/profile/:id", controller.getBreederProfileById);
router.delete("/manage/breeder/profile/:id", authenticate, highRole, controller.manageDeleteBreederProfile);

router.get("/statistics/users", authenticate, adminOnly, controller.getStatisticsUsers);

module.exports = router;
