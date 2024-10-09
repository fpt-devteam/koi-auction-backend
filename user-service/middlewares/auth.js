// const UserRole = require('../models/userRole');

const authenticate = (req, res, next) => {
   const userId = req.headers["uid"];
   const userRoleId = req.headers["uri"];

   if (!userId) {
      return res.status(401).json({ message: 'Unauthorized' });
   }
   
   req.user = { userId, userRoleId };
   next();
}

const verifyRole = (role) => {
   return async (req, res, next) => {
      if (req.user.UserRoleId !== role) {
         return res.status(403).json({ message: 'Forbidden' });
      }
      next();
   }
}

module.exports = {
   authenticate,
   verifyRole
}