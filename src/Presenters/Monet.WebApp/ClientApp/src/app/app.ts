import { AsyncPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, OnInit, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { Store } from '@ngrx/store';

import { VersionActions } from './state/version/version.actions';
import {
  selectVersionError,
  selectVersionLoading,
  selectVersionValue
} from './state/version/version.selectors';

@Component({
  selector: 'app-root',
  imports: [AsyncPipe, RouterOutlet],
  templateUrl: './app.html',
  styleUrl: './app.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class App implements OnInit {
  private readonly store = inject(Store);

  readonly version$ = this.store.select(selectVersionValue);
  readonly versionLoading$ = this.store.select(selectVersionLoading);
  readonly versionError$ = this.store.select(selectVersionError);

  ngOnInit(): void {
    this.store.dispatch(VersionActions.loadVersion());
  }

  refresh(): void {
    this.store.dispatch(VersionActions.loadVersion());
  }
}
