export const environment = {
  production: true,
  baseUrl: 'http://localhost:5000',
  oAuthConfig: {
    issuer: 'http://localhost:5000',
    redirectUri: 'http://localhost:4200',
    clientId: 'FamilyChat_App',
    responseType: 'code',
    scope: 'offline_access openid profile email phone',
    requireHttps: false,
    skipIssuerCheck: true
  },
  apis: {
    default: {
      url: 'http://localhost:5000',
      rootNamespace: 'FamilyChat'
    }
  }
};
