import { Environment } from '@abp/ng.core';

const baseUrl = 'http://MeetAdminWeb:4200';

export const environment = {
  production: true,
  application: {
    baseUrl,
    name: 'Admin',
    logoUrl: '',
  },
  oAuthConfig: {
    issuer: 'http://MeetApi:5000/',
    redirectUri: baseUrl,
    clientId: 'Admin_App',
    responseType: 'code',
    scope: 'offline_access Admin',
    requireHttps: false
  },
  apis: {
    default: {
      url: 'http://MeetApi:5000',
      rootNamespace: 'FamilyMeet.Admin',
    },
  },
} as Environment;
