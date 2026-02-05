import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';

import { AccountsService } from '../services/accounts.service';

@Component({
  selector: 'app-accounts',
  templateUrl: './accounts.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AccountsComponent {
  private readonly accountsService = inject(AccountsService);

  readonly accounts = this.accountsService.accounts;
  readonly isModalOpen = signal(false);
  readonly accountName = signal('');
  readonly accountNumber = signal('');
  readonly formError = signal('');

  openModal(): void {
    this.formError.set('');
    this.accountName.set('');
    this.accountNumber.set('');
    this.isModalOpen.set(true);
  }

  closeModal(): void {
    this.isModalOpen.set(false);
  }

  updateAccountName(value: string): void {
    this.accountName.set(value);
  }

  updateAccountNumber(value: string): void {
    this.accountNumber.set(value);
  }

  submit(): void {
    const name = this.accountName().trim();
    const number = this.accountNumber().trim();

    if (!name || !number) {
      this.formError.set('Merci de renseigner un nom et un numéro de compte.');
      return;
    }

    this.accountsService.addAccount(name, number);
    this.isModalOpen.set(false);
  }
}
