import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { GlobalProgressBarComponent } from './shared/components/global-progress-bar/global-progress-bar.component';

@Component({
    selector: 'app-root',
    imports: [RouterOutlet, GlobalProgressBarComponent],
    templateUrl: './app.component.html',
    styleUrl: './app.component.scss'
})
export class AppComponent {
  title = 'supermarket-web';
}
