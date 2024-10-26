const passport = require('passport');
const GoogleStrategy = require('passport-google-oauth2').Strategy;

passport.serializeUser((user, done) => {
   done(null, user);
})
passport.deserializeUser(function (user, done) {
   done(null, user);
});

passport.use(new GoogleStrategy({
   clientID: "896701632794-fsdbrdh9i80qnid08gj9dv01pst91bbr.apps.googleusercontent.com", // Your Credentials here.
   clientSecret: "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", // Your Credentials here.
   callbackURL: "http://localhost:5173",
   passReqToCallback: true
},
   function (request, accessToken, refreshToken, profile, done) {
      return done(null, profile);
   }
));

module.exports = passport;