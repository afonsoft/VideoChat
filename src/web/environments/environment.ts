import { Environment } from '@abp/ng.core';

const baseUrl = 'http://localhost:4200';

export const environment = {
  production: false,
  application: {
    baseUrl,
    name: 'FamilyChat',
    logoUrl: '',
  },
  oAuthConfig: {
    issuer: 'https://localhost:5000/',
    redirectUri: baseUrl,
    clientId: 'FamilyChat_App',
    responseType: 'code',
    scope: 'offline_access FamilyChat',
    requireHttps: false, // Para desenvolvimento
  },
  apis: {
    default: {
      url: 'https://localhost:5000',
      rootNamespace: 'FamilyChat',
    },
  },
} as Environment;
