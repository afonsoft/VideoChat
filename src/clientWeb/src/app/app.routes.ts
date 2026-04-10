import { Routes } from '@angular/router';
import { LoginComponent } from './components/login/login.component';
import { ChatComponent } from './components/chat/chat.component';
import { VideoCallComponent } from './components/video-call/video-call.component';
import { AuthGuard } from './guards/auth.guard';

export const routes: Routes = [
  {
    path: '',
    redirectTo: '/login',
    pathMatch: 'full'
  },
  {
    path: 'login',
    component: LoginComponent,
    title: 'Login - FamilyMeet'
  },
  {
    path: 'chat',
    component: ChatComponent,
    canActivate: [AuthGuard],
    title: 'Chat - FamilyMeet'
  },
  {
    path: 'video-call',
    component: VideoCallComponent,
    canActivate: [AuthGuard],
    title: 'Video Call - FamilyMeet'
  },
  {
    path: '**',
    redirectTo: '/login'
  }
];
