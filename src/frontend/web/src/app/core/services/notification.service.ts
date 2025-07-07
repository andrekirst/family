import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';

export interface NotificationMessage {
  id: string;
  type: 'success' | 'error' | 'warning' | 'info';
  message: string;
  duration?: number;
  timestamp: number;
}

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  private notifications$ = new BehaviorSubject<NotificationMessage[]>([]);
  
  get notifications(): Observable<NotificationMessage[]> {
    return this.notifications$.asObservable();
  }

  showSuccess(message: string, duration: number = 5000): void {
    this.addNotification('success', message, duration);
  }

  showError(message: string, duration: number = 7000): void {
    this.addNotification('error', message, duration);
  }

  showWarning(message: string, duration: number = 6000): void {
    this.addNotification('warning', message, duration);
  }

  showInfo(message: string, duration: number = 4000): void {
    this.addNotification('info', message, duration);
  }

  remove(id: string): void {
    const currentNotifications = this.notifications$.value;
    const updatedNotifications = currentNotifications.filter(n => n.id !== id);
    this.notifications$.next(updatedNotifications);
  }

  clear(): void {
    this.notifications$.next([]);
  }

  private addNotification(type: NotificationMessage['type'], message: string, duration: number): void {
    const notification: NotificationMessage = {
      id: this.generateId(),
      type,
      message,
      duration,
      timestamp: Date.now()
    };

    const currentNotifications = this.notifications$.value;
    this.notifications$.next([...currentNotifications, notification]);

    // Auto-remove after duration
    if (duration > 0) {
      setTimeout(() => {
        this.remove(notification.id);
      }, duration);
    }
  }

  private generateId(): string {
    return Math.random().toString(36).substring(2, 9);
  }
}