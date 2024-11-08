const Role = require('../models/role');
const { verify } = require('jsonwebtoken');
require('dotenv').config();

const authenticate = (req, res, next) => {
   if (req.headers["authorization"]) {
      console.log(req.headers["authorization"]);
      const serviceToken = req.headers["authorization"].split(' ')[1];
      if (serviceToken !== process.env.SERVICE_TOKEN) {
         return next();
      }
      try {
         const decoded = verify(serviceToken, process.env.JWT_SECRET);
         console.log(decoded);
         req.user = { UserId: decoded.UserId, UserRoleId: decoded.UserRoleId };
         return next();
      } catch (error) {
         return res.status(401).json({ message: 'Unauthorized service token' });
      }
   }

   const UserId = req.headers["uid"];
   const UserRoleId = req.headers["uri"];

   if (!UserId) {
      return res.status(401).json({ message: 'Unauthorized' });
   }
   
   req.user = { UserId, UserRoleId };
   next();
}

const verifyRole = (role) => {
   return async (req, res, next) => {
      const userRole = await Role.findOne({ where: { RoleId: req.user.UserRoleId } });
      if (!role.includes(userRole.RoleName)) {
         return res.status(403).json({ message: 'Forbidden' });
      }
      next();
   }
}

module.exports = {
   authenticate,
   verifyRole
}