const Role = require('../models/role');

const authenticate = (req, res, next) => {
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