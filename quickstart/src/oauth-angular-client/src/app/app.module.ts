import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { HttpClientModule } from '@angular/common/http';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { CoreModule } from './core/core.module';
import { HomeModule } from './home/home.module';
import { ConfigService } from './shared/config.service';
import { SharedModule } from './shared/shared.module';
import { ShellModule } from './shell/shell.module';
import { AccountModule } from './account/account.module';

@NgModule({
  declarations: [AppComponent],
  imports: [
    BrowserModule,
    HttpClientModule,
    AppRoutingModule,
    CoreModule,
    AccountModule,
    HomeModule,
    ShellModule,
    SharedModule,
  ],
  providers: [ConfigService],
  bootstrap: [AppComponent],
})
export class AppModule {}
