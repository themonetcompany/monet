import { Injectable, signal } from '@angular/core';

type FocusType = 'project' | 'area';

@Injectable({ providedIn: 'root' })
export class FocusDefaultAccountService {
  private readonly defaults = signal<Record<string, string>>({});

  getDefaultAccount(type: FocusType, id: string): string | null {
    return this.defaults()[`${type}:${id}`] ?? null;
  }

  setDefaultAccount(type: FocusType, id: string, accountId: string): void {
    this.defaults.update((current) => ({
      ...current,
      [`${type}:${id}`]: accountId
    }));
  }
}
