import { Routes } from '@angular/router';

export const appRoutes: Routes = [
  {
    path: '',
    redirectTo: '/chat',
    pathMatch: 'full'
  },
  {
    path: 'chat',
    loadComponent: () => import('../app/components/chat-room/chat-room.component').then(m => m.ChatRoomComponent),
    title: 'Chat'
  }
];
