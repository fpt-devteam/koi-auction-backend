const User = require("../models/user");
const Wallet = require("../models/wallet");
const BreederDetail = require("../models/breeder");
const argon2 = require("argon2");
const { Op } = require("sequelize");
const { sign, verify } = require("jsonwebtoken");
const sendEmail = require("../utils/sendEmail");
const passport = require("../utils/passport");
const Province = require("../models/provinces");
const District = require("../models/districts");
const Ward = require("../models/wards");

const profile = async (req, res) => {
   try {
      const user = await User.findOne({ where: { UserId: req.user.UserId } });
      res.status(200).json({
         UserId: user.UserId,
         Username: user.Username,
         FirstName: user.FirstName,
         LastName: user.LastName,
         Phone: user.Phone,
         Email: user.Email,
         Active: user.Active,
         UserRoleId: user.UserRoleId,
         ProvinceCode: user.ProvinceCode,
         DistrictCode: user.DistrictCode,
         WardCode: user.WardCode,
      });
   } catch (err) {
      console.log(err);
      res.status(500).json({ message: "Internal server error" });
   }
};

const googleAuth = async (req, res) => {
   const { Email, FirstName, LastName, GoogleId } = req.body;
   const existUser = await User.findOne({ where: { GoogleId: GoogleId } });

   if (existUser) {
      const accessToken = sign(
         { UserId: existUser.UserId, UserRoleId: existUser.UserRoleId },
         process.env.JWT_SECRET
      );
      const signInExpire = 1000 * 60 * 60 * 24;
      res.cookie("access-token", accessToken, {
         httpOnly: true,
         secure: true,
         sameSite: "none",
         maxAge: signInExpire,
      });
      return res.status(200).json({ message: "Login successful", user: existUser });
   }

   const existUserByEmail = await User.findOne({ where: { Email: Email } });

   if (existUserByEmail) {
      return res.status(409).json({ message: "Email already exists" });
   }

   const existUserByGoogleId = await User.findOne({ where: { GoogleId: GoogleId } });

   if (existUserByGoogleId) {
      const accessToken = sign(
         { UserId: existUserByGoogleId.UserId, UserRoleId: existUserByGoogleId.UserRoleId },
         process.env.JWT_SECRET
      );
      const signInExpire = 1000 * 60 * 60 * 24;
      res.cookie("access-token", accessToken, {
         httpOnly: true,
         secure: true,
         sameSite: "none",
         maxAge: signInExpire,
      });
      return res.status(200).json({ message: "Login successful", user: existUserByGoogleId });
   }

   let userId;

   try {
      await User.create({
         Username: Email,
         Email: Email,
         FirstName: FirstName,
         LastName: LastName,
         GoogleId: GoogleId,
         Active: true,
         UserRoleId: 1,
         CreatedAt: new Date(),
         UpdatedAt: new Date(),
      }).then((user) => {
         userId = user.UserId;
      });

      await Wallet.create({
         UserId: userId,
         Balance: 0,
         CreatedAt: new Date(),
         UpdatedAt: new Date(),
      });

      const accessToken = sign(
         { UserId: userId, UserRoleId: 1 },
         process.env.JWT_SECRET
      );

      res.cookie("access-token", accessToken, {
         httpOnly: true,
         secure: true,
         sameSite: "none",
         maxAge: 30 * 24 * 60 * 60 * 1000,
      });

      res.status(200).json({ 
         message: "Login successful", 
         user: { UserId: userId, Username: Email, Email: Email, FirstName: FirstName, LastName: LastName, Active: true }
      });
   } catch (err) {
      console.log(err);
      res.status(500).json({ message: "Internal server error" });
   }
}

const googleSuccess = async (req, res) => {
   try {
      if (req.user) {
         return res.status(200).json({ message: "Google authentication successful", user: req.user });
      }
      res.status(401).json({ message: "Google authentication failed" });
   } catch (err) {
      console.log(err);
      res.status(500).json({ message: "Internal server error" });
   }
}

const googleFailure = async (req, res) => {
   try {
      res.status(401).json({ message: "Google authentication failed" });
   } catch (err) {
      console.log(err);
      res.status(500).json({ message: "Internal server error" });
   }
}

const login = async (req, res) => {
   try {
      const reqBody = req.body;
      if (!reqBody.Username || !reqBody.Password) {
         return res.status(400).json({ message: "All fields are required" });
      }

      const user = await User.findOne({ where: { Username: reqBody.Username } });

      if (!user) {
         return res.status(401).json({ message: "Username or Password is incorrect" });
      }

      if (user.GoogleId) {
         return res.status(401).json({ message: "Please login with Google" });
      }

      if (!user.Active) {
         return res.status(401).json({ message: "User is not available" });
      }

      const match = await argon2.verify(user.Password, reqBody.Password);

      if (match) {
         const accessToken = sign(
            { UserId: user.UserId, UserRoleId: user.UserRoleId },
            process.env.JWT_SECRET
         );
         const signInExpire = reqBody.RememberMe
            ? 1000 * 60 * 60 * 24 * 30
            : 1000 * 60 * 60 * 24;
         res.cookie("access-token", accessToken, {
            httpOnly: true,
            secure: true,
            sameSite: "none",
            maxAge: signInExpire,
         });
         return res.status(200).json({ message: "Login successful", user: user });
      }
      res.status(401).json({ message: "Username or Password is incorrect" });
   } catch (err) {
      console.log(err);
      res.status(500).json({ message: "Internal server error" });
   }
};

const register = async (req, res) => {
   try {
      const { Username, Password, FirstName, LastName, Phone, Email } = req.body;
      const existUser = await User.findOne({
         where: {
            [Op.or]: [{ Username: Username }, { Email: Email }, { Phone: Phone }],
         },
      });

      if (!Username || !Password || !FirstName || !LastName || !Phone || !Email) {
         return res.status(400).json({ message: "All fields are required" });
      }

      if (existUser?.Username == Username) return res.status(400).json({ message: "Username already exists" });
      if (existUser?.Email == Email) return res.status(400).json({ message: "Email already exists" });
      if (existUser?.Phone == Phone) return res.status(400).json({ message: "Phone already exists" });

      const hashedPassword = await argon2.hash(Password, 10);

      let userId;
      await User.create({
         Username: Username,
         Password: hashedPassword,
         FirstName: FirstName,
         LastName: LastName,
         Phone: Phone,
         Email: Email,
         Active: true,
         UserRoleId: 1,
         CreatedAt: new Date(),
         UpdatedAt: new Date(),
      }).then((user) => {
         userId = user.UserId;
      });

      await Wallet.create({
         UserId: userId,
         Balance: 0,
         CreatedAt: new Date(),
         UpdatedAt: new Date(),
      });

      res.status(201).json({ message: "User Created" });
   } catch (err) {
      console.log(err);
      res.status(500).json({ message: "Internal server error" });
   }
};

const updateProfile = async (req, res) => {
   try {
      const { Username, FirstName, LastName, Phone, Email, ProvinceCode, DistrictCode, WardCode } = req.body;
      await User.update(
         {
            Username: Username,
            FirstName: FirstName,
            LastName: LastName,
            Phone: Phone,
            Email: Email,
            ProvinceCode: ProvinceCode,
            DistrictCode: DistrictCode,
            WardCode: WardCode,
            CreatedAt: new Date(),
            UpdatedAt: new Date(),
         },
         { where: { UserId: req.user.UserId } }
      );
      res.status(201).json({ message: "User Updated" });
   } catch (err) {
      console.log(err);
      res.status(500).json({ message: "Internal server error" });
   }
};

const updatePassword = async (req, res) => {
   try {
      const { Password } = req.body;
      if (!Password) {
         return res.status(400).json({ message: "Password is required" });
      }
      const hashedPassword = await argon2.hash(Password, 10);
      await User.update(
         { Password: hashedPassword },
         { where: { UserId: req.user.UserId } }
      );
      res.status(201).json({ message: "Password Updated" });
   } catch (err) {
      console.log(err);
      res.status(500).json({ message: "Internal server error" });
   }
};

const forgotPassword = async (req, res) => {
   try {
      const { Email } = req.body;
      const user = await User.findOne({ where: { Email: Email } });

      if (!user) {
         return res.status(404).json({ message: "User not found" });
      }
      const token = sign({ UserId: user.UserId }, process.env.JWT_SECRET, {
         expiresIn: "15m",
      });
      const resetLink = `http://localhost:3000/reset-Password/${token}`;
      const subject = "Password Reset Link";
      const text = `Click on the link to reset your Password: ${resetLink}`;
      await sendEmail(Email, subject, text);
      res.status(200).json({ message: "Password reset link sent to your Email" });
   } catch (err) {
      console.log(err);
      res.status(500).json({ message: "Internal server error" });
   }
};

const resetPassword = async (req, res) => {
   try {
      const { Password } = req.body;
      const token = req.params.token;

      verify(token, process.env.JWT_SECRET, async (err, decoded) => {
         if (err) {
            return res.status(401).json({ message: "Invalid or expired token" });
         }
         const user = await User.findByPk(decoded.UserId);

         if (!user) {
            return res.status(404).json({ message: "User not found" });
         }

         const hashedPassword = await argon2.hash(Password, 10);

         await User.update(
            { Password: hashedPassword },
            { where: { UserId: user.UserId } }
         );
      });
      res.status(201).json({ message: "Password Updated" });
   } catch (err) {
      console.log(err);
      res.status(500).json({ message: "Internal server error" });
   }
};

const logout = async (req, res) => {
   res.clearCookie("access-token");
   res.status(200).json({ message: "Logout successful" });
};

const deleteAccount = async (req, res) => {
   try {
      await User.update(
         { Active: false },
         { where: { UserId: req.user.UserId } }
      );
      res.status(201).json({ message: "User Deleted" });
   } catch (err) {
      console.log(err);
      res.status(500).json({ message: "Internal server error" });
   }
};

const getAllProfiles = async (req, res) => {
   try {
      const users = await User.findAll();
      if (!users) {
         return res.status(404).json({ message: "Users not found" });
      }
      res.status(200).json(users);
   } catch (err) {
      console.log(err);
      res.status(500).json({ message: "Internal server error" });
   }
};

const getProfileById = async (req, res) => {
   try {
      const user = await User.findByPk(req.params.id);
      if (!user) {
         return res.status(404).json({ message: "User not found" });
      }
      res.status(200).json(user);
   } catch (err) {
      console.log(err);
      res.status(500).json({ message: "Internal server error" });
   }
};

const manageDeleteProfile = async (req, res) => {
   try {
      const user = await User.findByPk(req.params.id);
      if (!user) {
         return res.status(404).json({ message: "User not found" });
      }

      await User.update(
         { Active: false },
         { where: { UserId: req.params.id } }
      );
      res.status(200).json({ message: "User Deleted" });
   } catch (err) {
      console.log(err);
      res.status(500).json({ message: "Internal server error" });
   }
};

const manageCreateProfile = async (req, res) => {
   try {
      const { Username, Password, FirstName, LastName, Phone, Email, UserRoleId, Active } =
         req.body;

      if (!Username || !Password || !FirstName || !LastName || !Phone || !Email) {
         return res.status(400).json({ message: "All fields are required" });
      }

      const existUser = await User.findOne({
         where: {
            [Op.or]: [{ Username: Username }, { Email: Email }, { Phone: Phone }],
         },
      });

      if (existUser?.Username == Username) return res.status(400).json({ message: "Username already exists" });
      if (existUser?.Email == Email) return res.status(400).json({ message: "Email already exists" });
      if (existUser?.Phone == Phone) return res.status(400).json({ message: "Phone already exists" });

      const hashedPassword = await argon2.hash(Password, 10);
      let UserId = null;
      await User.create({
         Username: Username,
         Password: hashedPassword,
         FirstName: FirstName,
         LastName: LastName,
         Phone: Phone,
         Email: Email,
         Active: Active || true,
         UserRoleId: UserRoleId || 1,
         CreatedAt: new Date(),
         UpdatedAt: new Date(),
      }).then((user) => {
         UserId = user.UserId;
      });

      await Wallet.create({
         UserId: UserId,
         Balance: 0,
         CreatedAt: new Date(),
         UpdatedAt: new Date(),
      });

      if (UserRoleId == 2) {
         const { FarmName, Certificate, About } = req.body;

         if (!FarmName || !Certificate) {
            return res.status(400).json({ message: "All fields are required" });
         }

         await BreederDetail.create({
            BreederId: UserId,
            FarmName: FarmName,
            Certificate: Certificate,
            About: About,
         });

         console.log("Breeder Profile Created");
      }

      res.status(201).json({ message: "User Created" });
   } catch (err) {
      console.log(err);
      res.status(500).json({ message: "Internal server error" });
   }
};

const getBreederProfile = async (req, res) => {
   try {
      const breeder = await BreederDetail.findByPk(req.user.UserId);
      if (!breeder) {
         return res.status(404).json({ message: "Breeder Profile not found" });
      }
      res.status(200).json(breeder);
   } catch (err) {
      console.log(err);
      res.status(500).json({ message: "Internal server error" });
   }
};

const getAllBreederProfiles = async (req, res) => {
   try {
      const breeders = await BreederDetail.findAll();
      if (!breeders) {
         return res.status(404).json({ message: "Breeder Profiles not found" });
      }
      res.status(200).json(breeders);
   } catch (err) {
      console.log(err);
      res.status(500).json({ message: "Internal server error" });
   }
};

const getBreederProfileById = async (req, res) => {
   try {
      const breeder = await BreederDetail.findByPk(req.params.id);
      if (!breeder) {
         return res.status(404).json({ message: "Breeder Profile not found" });
      }
      res.status(200).json(breeder);
   } catch (err) {
      console.log(err);
      res.status(500).json({ message: "Internal server error" });
   }
};

const manageUpdateProfile = async (req, res) => {
   try {
      const { Username, FirstName, LastName, Phone, Email, FarmName, Certificate, About, Active } = req.body;

      const user = await User.findByPk(req.params.id);
      if (!user) {
         return res.status(404).json({ message: "User not found" });
      }

      await User.update(
         {
            Username: Username,
            FirstName: FirstName,
            LastName: LastName,
            Phone: Phone,
            Email: Email,
            Active: Active || true,
            UpdatedAt: new Date(),
         },
         { where: { UserId: req.params.id } }
      );

      await BreederDetail.update(
         {
            FarmName: FarmName,
            Certificate: Certificate,
            About: About,
         },
         { where: { BreederId: req.params.id } }
      );
      res.status(201).json({ message: "Breeder Profile Updated" });
   } catch (err) {
      console.log(err);
      res.status(500).json({ message: "Internal server error" });
   }
};

const manageDeleteBreederProfile = async (req, res) => {
   try {
      const user = await User.findByPk(req.params.id);
      if (!user) {
         return res.status(404).json({ message: "User not found" });
      }

      await User.update(
         { Active: false },
         { where: { UserId: req.params.id } }
      );
      res.status(200).json({ message: "Breeder Profile Deleted" });
   } catch (err) {
      console.log(err);
      res.status(500).json({ message: "Internal server error" });
   }
};

const manageGetDetailProfile = async (req, res) => {
   const { id } = req.params;
   if (!id) {
      return res.status(400).json({ message: "ID is required" });
   }
   try {
      User.hasOne(BreederDetail, { foreignKey: "BreederId" });
      BreederDetail.belongsTo(User, { foreignKey: "BreederId" });
      const user = await User.findOne({
         where: { UserId: id },
         include: [{ model: BreederDetail }],
      });
      if (!user) {
         return res.status(404).json({ message: "Profile not found" });
      }

      res.status(200).json({
         UserId: user.UserId,
         Username: user.Username,
         FirstName: user.FirstName,
         LastName: user.LastName,
         Phone: user.Phone,
         Email: user.Email,
         Active: user.Active,
         UserRoleId: user.UserRoleId,
         FarmName: user.BreederDetail?.FarmName,
         Certificate: user.BreederDetail?.Certificate,
         About: user.BreederDetail?.About,
      });
   } catch (err) {
      console.log(err);
      res.status(500).json({ message: "Internal server error" });
   }
}

const getProvinces = async (req, res) => {
   try {
      const provinces = await Province.findAll();
      res.status(200).json(provinces);
   } catch (err) {
      console.log(err);
      res.status(500).json({ message: "Internal server error" });
   }
}

const getDistricts = async (req, res) => {
   try {
      const districts = await District.findAll();
      res.status(200).json(districts);
   } catch (err) {
      console.log(err);
      res.status(500).json({ message: "Internal server error" });
   }
}

const getWards = async (req, res) => {
   try {
      const wards = await Ward.findAll();
      res.status(200).json(wards);
   } catch (err) {
      console.log(err);
      res.status(500).json({ message: "Internal server error" });
   }
}

const getProvinceById = async (req, res) => {
   try {
      const province = await Province.findByPk(req.params.code);
      if (!province) {
         return res.status(404).json({ message: "Province not found" });
      }
      res.status(200).json(province);
   } catch (err) {
      console.log(err);
      res.status(500).json({ message: "Internal server error" });
   }
}

const getDistrictByProvinceId = async (req, res) => {
   try {
      const district = await District.findAll({
         where: { province_code: req.params.provinceId },   
      });
      if (!district) {
         return res.status(404).json({ message: "District not found" });
      }
      res.status(200).json(district);
   } catch (err) {
      console.log(err);
      res.status(500).json({ message: "Internal server error" });
   }
}

const getWardByDistrictId = async (req, res) => {
   try {
      const ward = await Ward.findAll({
         where: { district_code: req.params.districtId },   
      });
      if (!ward) {
         return res.status(404).json({ message: "Ward not found" });
      }
      res.status(200).json(ward);
   } catch (err) {
      console.log(err);
      res.status(500).json({ message: "Internal server error" });
   }
}


module.exports = {
   profile,
   googleAuth,
   login,
   googleSuccess,
   googleFailure,
   register,
   updateProfile,
   updatePassword,
   forgotPassword,
   resetPassword,
   deleteAccount,
   logout,
   getAllProfiles,
   getProfileById,
   manageGetDetailProfile,
   manageDeleteProfile,
   manageUpdateProfile,
   manageCreateProfile,
   getBreederProfile,
   getAllBreederProfiles,
   getBreederProfileById,
   manageDeleteBreederProfile,
   getProvinces,
   getDistricts,
   getWards,
   getProvinceById,
   getDistrictByProvinceId,
   getWardByDistrictId,
};
