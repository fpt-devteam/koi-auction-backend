const Role = require('../models/role');

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
      const userRole = await Role.findOne({ where: { RoleId: req.user.userRoleId } });
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