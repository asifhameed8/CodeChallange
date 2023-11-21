import { AuthService } from '../../services/auth.service';
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import ValidateForm from '../../helpers/validationform';
import { Router } from '@angular/router';
import { UserStoreService } from 'src/app/services/user-store.service';
import { ApiService } from 'src/app/services/api.service';

@Component({
  selector: 'app-add-user',
  templateUrl: './add-user.component.html',
  styleUrls: ['./add-user.component.css']
})
export class AddUserComponent implements OnInit {

  public addUserForm!: FormGroup;
  public role!:string;
  public users:any = [];
  type: string = 'password';
  isText: boolean = false;
  eyeIcon:string = "fa-eye-slash"
  constructor(private api : ApiService, private fb : FormBuilder, private auth: AuthService, private router: Router,private userStore: UserStoreService,) { }

  ngOnInit() {
    this.addUserForm = this.fb.group({
      firstName:['', Validators.required],
      lastName:['', Validators.required],
      userName:['', Validators.required],
      role:['', Validators.required],
      managerId:['-1', Validators.required],
      email:['', Validators.required],
      password:['', Validators.required]
    });

    this.api.getUsers()
    .subscribe(res=>{
    this.users = res;
    });

    this.userStore.getRoleFromStore()
    .subscribe(val=>{
      const roleFromToken = this.auth.getRoleFromToken();
      this.role = val || roleFromToken;
    })
  }

  hideShowPass(){
    this.isText = !this.isText;
    this.isText ? this.eyeIcon = 'fa-eye' : this.eyeIcon = 'fa-eye-slash'
    this.isText ? this.type = 'text' : this.type = 'password'
  }

  onSubmit() {
    if (this.addUserForm.valid) {
      console.log(this.addUserForm.value);
      let signUpObj = {
        ...this.addUserForm.value,
        role:'',
        token:''
      }
      this.auth.signUp(signUpObj)
      .subscribe({
        next:(res=>{
          console.log(res.message);
          this.addUserForm.reset();
          this.router.navigate(['dashboard']);
          alert(res.message)
        }),
        error:(err=>{
          alert(err?.error.message)
        })
      })
    } else {
      ValidateForm.validateAllFormFields(this.addUserForm); //{7}
    }
  }

}
