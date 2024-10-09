const swaggerJsDoc = require('swagger-jsdoc');
const swaggerUi = require('swagger-ui-express');
const path = require('path');

// Swagger definition
const swaggerDefinition = {
   openapi: '3.0.0',
   info: {
      title: 'User Service API',
      version: '1.0.0',
      description: 'API for managing user profiles, login, registration, and account updates.',
   },
   servers: [
      {
         url: 'http://localhost:3000/user-service',
      },
   ],
};

// Options for the swagger docs
const options = {
   swaggerDefinition,
   apis: [path.join(__dirname, '../docs/swagger.yaml')], // Path to the API docs
};

// Initialize swagger-jsdoc
const swaggerSpec = swaggerJsDoc(options);

const setupSwaggerDocs = (app) => {
   app.use('/api-docs', swaggerUi.serve, swaggerUi.setup(swaggerSpec));
};

module.exports = setupSwaggerDocs;
