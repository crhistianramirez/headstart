import { Component, OnInit } from '@angular/core'
import { FormGroup, Validators, FormControl } from '@angular/forms'
import { MeUser } from 'ordercloud-javascript-sdk'
import { AppConfig } from 'src/app/models/environment.types'
import { ShopperContextService } from 'src/app/services/shopper-context/shopper-context.service'
import {
  ValidateName,
  ValidateEmail,
  ValidatePhone,
  ValidateStrongPassword,
  ValidateFieldMatches,
} from '../../../validators/validators'

@Component({
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.scss'],
})
export class OCMRegister implements OnInit {
  form: FormGroup
  appName: string

  constructor(
    private context: ShopperContextService,
    public appConfig: AppConfig
  ) {}

  // TODO: validation isn't working
  ngOnInit(): void {
    this.appName = this.context.appSettings.appname
    this.form = new FormGroup({
      Username: new FormControl('', Validators.required),
      FirstName: new FormControl('', [Validators.required, ValidateName]),
      LastName: new FormControl('', [Validators.required, ValidateName]),
      Email: new FormControl('', [Validators.required, ValidateEmail]),
      Phone: new FormControl('', ValidatePhone),
      Password: new FormControl('', [
        Validators.required,
        ValidateStrongPassword,
      ]),
      ConfirmPassword: new FormControl('', [
        Validators.required,
        ValidateFieldMatches('Password'),
      ]),
    })
  }

  // TODO: requires anonymous token, but not checked for here
  async onSubmit(): Promise<void> {
    const me: MeUser = this.form.value
    me.Active = true
    await this.context.authentication.register(me)
    this.context.router.toLogin()
  }
}
