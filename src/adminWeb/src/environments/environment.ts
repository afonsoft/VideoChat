import { Environment } from '@abp/ng.core';

const baseUrl = 'http://localhost:4200';

export const environment = {
  production: false,
  application: {
    baseUrl,
    name: 'Admin',
    logoUrl: '',
  },
  oAuthConfig: {
    issuer: 'https://localhost:44350/',
    redirectUri: baseUrl,
    clientId: 'Admin_App',
    responseType: 'code',
    scope: 'offline_access Admin',
    requireHttps: true,
  },
  apis: {
    default: {
      url: 'https://localhost:44350',
      rootNamespace: 'FamilyMeet.Admin',
    },
  },
} as Environment;
