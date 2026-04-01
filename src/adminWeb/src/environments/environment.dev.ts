import { Environment } from '@abp/ng.core';

const baseUrl = 'http://localhost:4201';

export const environment = {
  production: false,
  application: {
    baseUrl,
    name: 'Admin',
    logoUrl: '',
  },
  oAuthConfig: {
    issuer: 'http://localhost:5000/',
    redirectUri: baseUrl,
    clientId: 'Admin_App',
    responseType: 'code',
    scope: 'offline_access Admin',
    requireHttps: false,
  },
  apis: {
    default: {
      url: 'http://localhost:5000',
      rootNamespace: 'FamilyMeet.Admin',
    },
  },
} as Environment;
