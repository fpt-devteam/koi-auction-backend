const User = require("../models/user");
const BreederDetail = require("../models/breeder");
const argon2 = require("argon2");
const { Op } = require("sequelize");
const { sign, verify } = require("jsonwebtoken");
const sendEmail = require("../utils/sendEmail");

const profile = async (req, res) => {
   try {
      const user = await User.findOne({ where: { UserId: req.user.userId } });
      res.status(200).json({
         UserId: user.UserId,
         Username: user.Username,
         FirstName: user.FirstName,
         LastName: user.LastName,
         Phone: user.Phone,
         Email: user.Email,
         Balance: user.Balance,
         UserRoleId: user.UserRoleId,
      });
   } catch (err) {
      console.log(err);
      res.status(500).json({ message: "Internal server error" });
   }
};

const login = async (req, res) => {
   try {
      const reqBody = req.body;
      const user = await User.findOne({ where: { Username: reqBody.username } });

      if (!user) {
         return res.status(401).json({ message: "Username or Password is incorrect" });
      }

      if (!user.Active) {
         return res.status(401).json({ message: "User is not available" });
      }

      const match = await argon2.verify(user.Password, reqBody.password);

      if (match) {
         const accessToken = sign(
            { userId: user.UserId, userRoleId: user.UserRoleId },
            process.env.JWT_SECRET
         );
         const signInExpire = reqBody["remember-me"]
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
      const { username, password, firstname, lastname, phone, email } = req.body;
      const existUser = await User.findOne({
         where: {
            [Op.or]: [{ Username: username }, { Email: email }, { Phone: phone }],
         },
      });

      if (!username || !password || !firstname || !lastname || !phone || !email) {
         return res.status(400).json({ message: "All fields are required" });
      }

      if (existUser?.Username == username) return res.status(400).json({ message: "Username already exists" });
      if (existUser?.Email == email) return res.status(400).json({ message: "Email already exists" });
      if (existUser?.Phone == phone) return res.status(400).json({ message: "Phone already exists" });

      const hashedPassword = await argon2.hash(password, 10);
      await User.create({
         Username: username,
         Password: hashedPassword,
         FirstName: firstname,
         LastName: lastname,
         Phone: phone,
         Email: email,
         Active: true,
         UserRoleId: 1,
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
      const { username, firstname, lastname, phone, email } = req.body;
      await User.update(
         {
            Username: username,
            FirstName: firstname,
            LastName: lastname,
            Phone: phone,
            Email: email,
            CreatedAt: new Date(),
            UpdatedAt: new Date(),
         },
         { where: { UserId: req.user.userId } }
      );
      res.status(201).json({ message: "User Updated" });
   } catch (err) {
      console.log(err);
      res.status(500).json({ message: "Internal server error" });
   }
};

const updatePassword = async (req, res) => {
   try {
      const { password } = req.body;
      if (!password) {
         return res.status(400).json({ message: "Password is required" });
      }
      const hashedPassword = await argon2.hash(password, 10);
      await User.update(
         { Password: hashedPassword },
         { where: { UserId: req.user.userId } }
      );
      res.status(201).json({ message: "Password Updated" });
   } catch (err) {
      console.log(err);
      res.status(500).json({ message: "Internal server error" });
   }
};

const forgotPassword = async (req, res) => {
   try {
      const { email } = req.body;
      const user = await User.findOne({ where: { Email: email } });

      if (!user) {
         return res.status(404).json({ message: "User not found" });
      }
      const token = sign({ userId: user.UserId }, process.env.JWT_SECRET, {
         expiresIn: "15m",
      });
      const resetLink = `http://localhost:3000/reset-password/${token}`;
      const subject = "Password Reset Link";
      const text = `Click on the link to reset your password: ${resetLink}`;
      await sendEmail(email, subject, text);
      res.status(200).json({ message: "Password reset link sent to your email" });
   } catch (err) {
      console.log(err);
      res.status(500).json({ message: "Internal server error" });
   }
};

const resetPassword = async (req, res) => {
   try {
      const { password } = req.body;
      const token = req.params.token;

      verify(token, process.env.JWT_SECRET, async (err, decoded) => {
         if (err) {
            return res.status(401).json({ message: "Invalid or expired token" });
         }
         const user = await User.findByPk(decoded.userId);

         if (!user) {
            return res.status(404).json({ message: "User not found" });
         }

         const hashedPassword = await argon2.hash(password, 10);

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
         { where: { UserId: req.user.userId } }
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

const manageUpdateProfile = async (req, res) => {
   try {
      const { username, firstname, lastname, phone, email, roleId, active } = req.body;
      await User.update(
         {
            Username: username,
            FirstName: firstname,
            LastName: lastname,
            Phone: phone,
            Email: email,
            UserRoleId: roleId,
            Active: active || true,
            CreatedAt: new Date(),
            UpdatedAt: new Date(),
         },
         { where: { UserId: req.params.id } }
      );
      res.status(201).json({ message: "User Updated" });
   } catch (err) {
      console.log(err);
      res.status(500).json({ message: "Internal server error" });
   }
};

const manageCreateProfile = async (req, res) => {
   try {
      const { username, password, firstname, lastname, phone, email, userRoleId } =
         req.body;
      const existUser = await User.findOne({
         where: {
            [Op.or]: [{ Username: username }, { Email: email }, { Phone: phone }],
         },
      });

      if (existUser?.Username == username) return res.status(400).json({ message: "Username already exists" });
      if (existUser?.Email == email) return res.status(400).json({ message: "Email already exists" });
      if (existUser?.Phone == phone) return res.status(400).json({ message: "Phone already exists" });

      const hashedPassword = await argon2.hash(password, 10);
      let userId = null;
      await User.create({
         Username: username,
         Password: hashedPassword,
         FirstName: firstname,
         LastName: lastname,
         Phone: phone,
         Email: email,
         Active: true,
         UserRoleId: userRoleId || 1,
         Balance: 0,
         CreatedAt: new Date(),
         UpdatedAt: new Date(),
      }).then((user) => {
         userId = user.UserId;
      });

      if (userRoleId == 2) {
         const { farmName, certificate, about } = req.body;
         await BreederDetail.create({
            BreederId: userId,
            FarmName: farmName,
            Certificate: certificate,
            About: about,
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
      const breeder = await BreederDetail.findByPk(req.user.userId);
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

const manageUpdateBreederProfile = async (req, res) => {
   try {
      const { farmName, location, contact } = req.body;
      await BreederDetail.update(
         {
            FarmName: farmName,
            Location: location,
            Contact: contact,
         },
         { where: { UserId: req.params.id } }
      );
      res.status(201).json({ message: "Breeder Profile Updated" });
   } catch (err) {
      console.log(err);
      res.status(500).json({ message: "Internal server error" });
   }
};

const manageDeleteBreederProfile = async (req, res) => {
   try {
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

module.exports = {
   profile,
   login,
   register,
   updateProfile,
   updatePassword,
   forgotPassword,
   resetPassword,
   deleteAccount,
   logout,
   getAllProfiles,
   getProfileById,
   manageDeleteProfile,
   manageUpdateProfile,
   manageCreateProfile,
   getBreederProfile,
   getAllBreederProfiles,
   getBreederProfileById,
   manageUpdateBreederProfile,
   manageDeleteBreederProfile,
};
