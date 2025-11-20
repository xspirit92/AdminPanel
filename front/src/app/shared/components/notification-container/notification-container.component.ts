import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AlertComponent } from '../../../shared/components/ui/alert/alert.component';
import { NotificationService } from '../../../shared/services/notification.service';

@Component({
  selector: 'app-notification-container',
  imports: [
    CommonModule,
    AlertComponent
  ],
  template: `
    <div class="fixed bottom-4 right-4 z-50 space-y-3 w-80 max-w-[calc(100vw-2rem)]">
      @for (notification of notifications$ | async; track notification.id) {
        <app-alert
          [variant]="notification.variant"
          [title]="notification.title"
          [message]="notification.message"
          [showLink]="notification.showLink || false"
          [linkHref]="notification.linkHref || '#'"
          [linkText]="notification.linkText || 'Learn more'"
          [showCloseButton]="notification.showCloseButton ?? true"
          (closed)="remove(notification.id)"
          (click)="remove(notification.id)"
          class="cursor-pointer transition-all duration-300 hover:opacity-90"
        />
      }
    </div>
  `,
  styles: ``
})
export class NotificationContainerComponent {
  private notificationService = inject(NotificationService);
  notifications$ = this.notificationService.notifications$;

  remove(id: number) {
    this.notificationService.remove(id);
  }
}