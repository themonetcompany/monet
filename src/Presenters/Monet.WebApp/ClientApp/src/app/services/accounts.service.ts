import { Injectable, signal } from '@angular/core';

export interface BankAccount {
  id: string;
  name: string;
  number: string;
}

@Injectable({ providedIn: 'root' })
export class AccountsService {
  readonly accounts = signal<BankAccount[]>([
    { id: 'account-1', name: 'Compte courant', number: 'FR76 3000 6000 0112 3456 7890 189' },
    { id: 'account-2', name: 'Épargne', number: 'FR14 2004 1010 0505 0001 3M02 606' }
  ]);

  addAccount(name: string, number: string): void {
    this.accounts.update((items) => [
      { id: `account-${items.length + 1}`, name, number },
      ...items
    ]);
  }
}
