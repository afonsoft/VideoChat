import { Environment } from '@abp/ng.core';

const baseUrl = 'http://localhost:4200';

export const environment = {
  production: false,
  application: {
    baseUrl,
    name: 'SimpleConnect',
    logoUrl: '',
  },
  oAuthConfig: {
    issuer: 'https://localhost:5000/',
    redirectUri: baseUrl,
    clientId: 'SimpleConnect_App',
    responseType: 'code',
    scope: 'offline_access SimpleConnect',
    requireHttps: false, // Para desenvolvimento
  },
  apis: {
    default: {
      url: 'https://localhost:5000',
      rootNamespace: 'SimpleConnect',
    },
  },
} as Environment;
