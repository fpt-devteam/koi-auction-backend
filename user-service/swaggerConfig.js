const swaggerJSDoc = require('swagger-jsdoc');

// Swagger definition
const swaggerDefinition = {
   openapi: '3.0.0',
   info: {
      title: 'User Service API',
      version: '1.0.0',
      description: 'API documentation for user registration, login, and profile management',
   },
   servers: [
      {
         url: 'http://127.0.0.1:3001',
      },
   ],
};

// Options for the swagger docs
const options = {
   swaggerDefinition,
   // Paths to files containing OpenAPI definitions
   apis: ['./index.js'], // Update with the correct file path where your routes are defined
};

const swaggerSpec = swaggerJSDoc(options);

module.exports = swaggerSpec;
