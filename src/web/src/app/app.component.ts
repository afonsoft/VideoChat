import { Component } from '@angular/core';
import { ChatRoomComponent } from './components/chat-room/chat-room.component';

@Component({
  selector: 'app-root',
  template: `
    <app-chat-room />
  `,
  imports: [ChatRoomComponent],
})
export class AppComponent {}
