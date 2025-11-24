import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

export interface Notification {
  id: number;
  variant: 'success' | 'error' | 'warning' | 'info';
  title: string;
  message: string;
  duration?: number;
  showLink?: boolean;
  linkHref?: string;
  linkText?: string;
  showCloseButton?: boolean;
}

export interface NotificationConfig {
  title?: string;
  message: string;
  duration?: number;
  showLink?: boolean;
  linkHref?: string;
  linkText?: string;
  showCloseButton?: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  private notificationsSubject = new BehaviorSubject<Notification[]>([]);
  public notifications$ = this.notificationsSubject.asObservable();
  
  private currentId = 0;

  private getDefaultTitle(variant: 'success' | 'error' | 'warning' | 'info'): string {
    const titles = {
      success: 'Успешно',
      error: 'Ошибка',
      warning: 'Внимание',
      info: 'Информация'
    };
    return titles[variant];
  }

  // Основной метод для показа уведомлений
  private show(notification: Omit<Notification, 'id'>) {
    const newNotification: Notification = {
      id: this.currentId++,
      duration: 5000, // По умолчанию 5 секунд
      showCloseButton: true, // По умолчанию true
      ...notification,
      title: notification.title || this.getDefaultTitle(notification.variant)
    };
    
    const currentNotifications = this.notificationsSubject.value;
    this.notificationsSubject.next([...currentNotifications, newNotification]);

    if (newNotification.duration && newNotification.duration > 0) {
      setTimeout(() => {
        this.remove(newNotification.id);
      }, newNotification.duration);
    }

    return newNotification.id;
  }

  // Упрощенные методы для быстрого вызова
  success(message: string): number;
  success(config: NotificationConfig): number;
  success(config: string | NotificationConfig): number {
    if (typeof config === 'string') {
      return this.show({
        variant: 'success',
        title: 'Успешно',
        message: config,
        duration: 5000,
        showCloseButton: true
      });
    } else {
      return this.show({
        variant: 'success',
        title: config.title || 'Успешно',
        message: config.message,
        duration: config.duration ?? 5000,
        showLink: config.showLink,
        linkHref: config.linkHref,
        linkText: config.linkText,
        showCloseButton: config.showCloseButton ?? true
      });
    }
  }

  error(message: string): number;
  error(config: NotificationConfig): number;
  error(config: string | NotificationConfig): number {
    if (typeof config === 'string') {
      return this.show({
        variant: 'error',
        title: 'Ошибка',
        message: config,
        duration: 5000,
        showCloseButton: true
      });
    } else {
      return this.show({
        variant: 'error',
        title: config.title || 'Ошибка',
        message: config.message,
        duration: config.duration ?? 5000,
        showLink: config.showLink,
        linkHref: config.linkHref,
        linkText: config.linkText,
        showCloseButton: config.showCloseButton ?? true
      });
    }
  }

  warning(message: string): number;
  warning(config: NotificationConfig): number;
  warning(config: string | NotificationConfig): number {
    if (typeof config === 'string') {
      return this.show({
        variant: 'warning',
        title: 'Внимание',
        message: config,
        duration: 5000,
        showCloseButton: true
      });
    } else {
      return this.show({
        variant: 'warning',
        title: config.title || 'Внимание',
        message: config.message,
        duration: config.duration ?? 5000,
        showLink: config.showLink,
        linkHref: config.linkHref,
        linkText: config.linkText,
        showCloseButton: config.showCloseButton ?? true
      });
    }
  }

  info(message: string): number;
  info(config: NotificationConfig): number;
  info(config: string | NotificationConfig): number {
    if (typeof config === 'string') {
      return this.show({
        variant: 'info',
        title: 'Информация',
        message: config,
        duration: 5000,
        showCloseButton: true
      });
    } else {
      return this.show({
        variant: 'info',
        title: config.title || 'Информация',
        message: config.message,
        duration: config.duration ?? 5000,
        showLink: config.showLink,
        linkHref: config.linkHref,
        linkText: config.linkText,
        showCloseButton: config.showCloseButton ?? true
      });
    }
  }

  remove(id: number) {
    const currentNotifications = this.notificationsSubject.value;
    const filteredNotifications = currentNotifications.filter(
      notification => notification.id !== id
    );
    this.notificationsSubject.next(filteredNotifications);
  }

  clear() {
    this.notificationsSubject.next([]);
  }
}