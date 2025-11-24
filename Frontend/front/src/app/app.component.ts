import { Component } from '@angular/core';
import { RouterModule } from '@angular/router';
import { NotificationContainerComponent } from './shared/components/notification-container/notification-container.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    RouterModule,
    NotificationContainerComponent
  ],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css',
})
export class AppComponent {
  title = 'CubArt';
}
