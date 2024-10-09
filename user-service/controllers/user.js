const User = require('../models/user');
const BreederDetail = require('../models/breeder');
const argon2 = require('argon2');
const { sign } = require('jsonwebtoken');

const profile = async (req, res) => {
   const user = await User
      .findOne({ where: { UserId: req.user.userId } })
      .catch(err => {
         console.log(err);
      });
   res.status(200).json(user);
}

const login = async (req, res) => {
   const reqBody = req.body;
   const user = await User
      .findOne({ where: { Username: reqBody.username } })
      .catch(err => {
         console.log(err);
      });

   if (!user) {
      return res.status(401).json({ message: 'Username or Password is incorrect' });
   }

   if (!user.Active) {
      return res.status(401).json({ message: 'User is not available' });
   }

   const match = await argon2.verify(user.Password, reqBody.password).catch(err => {
      console.log(err);
   });

   if (match) {
      const accessToken = sign({ userId: user.UserId, userRoleId: user.UserRoleId }, process.env.JWT_SECRET);
      const signInExpire = reqBody['remember-me'] ? 1000 * 60 * 60 * 24 * 30 : 1000 * 60 * 60 * 24;
      res.cookie('access-token', accessToken, { httpOnly: true, secure: true, sameSite: 'none', maxAge: signInExpire });
      return res.status(200).json({ message: 'Login successful', user: user });
   }
   res.status(401).json({ message: 'Username or Password is incorrect' });
}

const register = async (req, res) => {
   const { username, password, firstname, lastname, phone, email } = req.body;
   const existUser = await User
      .findOne({ where: { Username: username } })
      .catch(err => {
         console.log(err);
      });
   if (existUser) {
      return res.status(400).json({ message: 'User already exists' });
   }
   const existEmail = await User
      .findOne({ where: { Email: email } })
      .catch(err => {
         console.log(err);
      });
   if (existEmail) {
      return res.status(400).json({ message: 'Email already exists' });
   }
   const hashedPassword = await argon2.hash(password, 10);
   await User
      .create({
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
         UpdatedAt: new Date()
      }).catch(err => {
         console.log(err);
      });
   res.status(201).json({ message: "User Created" })
}

const updateProfile = async (req, res) => {
   const { username, firstname, lastname, phone, email } = req.body;
   await User
      .update({
         Username: username,
         FirstName: firstname,
         LastName: lastname,
         Phone: phone,
         Email: email,
         CreatedAt: new Date(),
         UpdatedAt: new Date()
      }, { where: { UserId: req.user.userId } }).catch(err => {
         console.log(err);
      });
   res.status(201).json({ message: "User Updated" })
}

const updatePassword = async (req, res) => {
   const { password } = req.body;
   if (!password) {
      return res.status(400).json({ message: 'Password is required' });
   }
   const hashedPassword = await argon2.hash(password, 10)
      .catch(err => {
         console.log(err);
      });
   await User
      .update({
         Password: hashedPassword
      }, { where: { UserId: req.user.userId } })
      .catch(err => {
         console.log(err);
      });
   res.status(201).json({ message: "Password Updated" })
}

const logout = async (req, res) => {
   res.clearCookie('access-token');
   res.status(200).json({ message: 'Logout successful' });
}

const deleteAccount = async (req, res) => {
   await User
      .update({
         Active: false
      }, { where: { UserId: req.user.userId } })
      .catch(err => {
         console.log(err);
      });
   res.status(201).json({ message: "User Deleted" })
}

const getAllProfiles = async (req, res) => {
   const users = await User.findAll();
   if (!users) {
      return res.status(404).json({ message: 'Users not found' });
   }
   res.status(200).json(users);
}

const getProfileById = async (req, res) => {
   const user = await User
      .findByPk(req.params.id)
      .catch(err => {
         console.log(err);
      });
   if (!user) {
      return res.status(404).json({ message: 'User not found' });
   }
   res.status(200).json(user);
}

const manageDeleteProfile = async (req, res) => {
   await User
      .destroy({ where: { UserId: req.params.id } })
      .catch(err => {
         console.log(err);
      });
   res.status(200).json({ message: "User Deleted" });
}

const manageUpdateProfile = async (req, res) => {
   const { username, firstname, lastname, phone, email, roleId } = req.body;
   await User
      .update({
         Username: username,
         FirstName: firstname,
         LastName: lastname,
         Phone: phone,
         Email: email,
         UserRoleId: roleId,
         CreatedAt: new Date(),
         UpdatedAt: new Date()
      }, { where: { UserId: req.params.id } })
      .catch(err => {
         console.log(err);
      });
   res.status(201).json({ message: "User Updated" })
}

const manageCreateProfile = async (req, res) => {
   const { username, password, firstname, lastname, phone, email, userRoleId } = req.body;
   const existUser = await User
      .findOne({ where: { Username: username } })
      .catch(err => {
         console.log(err);
      });
   if (existUser) {
      return res.status(400).json({ message: 'User already exists' });
   }
   const existEmail = await User
      .findOne({ where: { Email: email } })
      .catch(err => {
         console.log(err);
      });
   if (existEmail) {
      return res.status(400).json({ message: 'Email already exists' });
   }
   const hashedPassword = await argon2.hash(password, 10);
   let userId = null;
   await User
      .create({
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
         UpdatedAt: new Date()
      }).then(user => {
         userId = user.UserId;
      }).catch(err => {
         console.log(err);
      });

   if (userRoleId == 2) {
      const { farmName, certificate, about } = req.body;
      await BreederDetail
         .create({
            BreederId: userId,
            FarmName: farmName,
            Certificate: certificate,
            About: about
         }).catch(err => {
            console.log(err);
         });

      console.log('Breeder Profile Created');
   }

   res.status(201).json({ message: "User Created" })
}

const getBreederProfile = async (req, res) => {
   const breeder = await BreederDetail
      .findByPk(req.user.userId)
      .catch(err => {
         console.log(err);
      });
   if (!breeder) {
      return res.status(404).json({ message: 'Breeder Profile not found' });
   }
   res.status(200).json(breeder);
}

const getAllBreederProfiles = async (req, res) => {
   const breeders = await BreederDetail.findAll();
   if (!breeders) {
      return res.status(404).json({ message: 'Breeder Profiles not found' });
   }
   res.status(200).json(breeders);
}

const getBreederProfileById = async (req, res) => {
   const breeder = await BreederDetail
      .findByPk(req.params.id)
      .catch(err => {
         console.log(err);
      });
   if (!breeder) {
      return res.status(404).json({ message: 'Breeder Profile not found' });
   }
   res.status(200).json(breeder);
}

const manageUpdateBreederProfile = async (req, res) => {
   const { farmName, location, contact } = req.body;
   await BreederDetail
      .update({
         FarmName: farmName,
         Location: location,
         Contact: contact
      }, { where: { UserId: req.params.id } })
      .catch(err => {
         console.log(err);
      });
   res.status(201).json({ message: "Breeder Profile Updated" });
}

const manageDeleteBreederProfile = async (req, res) => {
   await BreederDetail.destroy({ where: { UserId: req.params.id } });
   res.status(200).json({ message: "Breeder Profile Deleted" });
}

module.exports = {
   profile,
   login,
   register,
   updateProfile,
   updatePassword,
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
   manageDeleteBreederProfile
}