
// I use the jQuery instead of $ to remember in the future
// 'jQuery' is equal to writing '$'
// here we make use of Jquery already made functions
// so we don't have to write stuff like Document.getElement and stuff

var Interests;
var Sexualities;
var Genders;
var Cities;
var Countries =
    [
        "Afghanistan",
        "Albania",
        "Algeria",
        "Andorra",
        "Angola",
        "Antigua&Barbuda",
        "Argentina",
        "Armenia",
        "Australia",
        "Austria",
        "Azerbaijan",
        "Bahamas",
        "Bahrain",
        "Bangladesh",
        "Barbados",
        "Belarus",
        "Belgium",
        "Belize",
        "Benin",
        "Bhutan",
        "Bolivia",
        "Bosnia&Herzegovina",
        "Botswana",
        "Brazil",
        "Brunei",
        "Bulgaria",
        "Burkina Faso",
        "Burundi",
        "Cabo Verde",
        "Cambodia",
        "Cameroon",
        "Canada",
        "Central African Republic",
        "Chad",
        "Chile",
        "China",
        "Colombia",
        "Comoros",
        "Congo (Congo-Brazzaville)",
        "Costa Rica",
        "Croatia",
        "Cuba",
        "Cyprus",
        "Czechia (Czech Republic)",
        "Denmark",
        "Djibouti",
        "Dominica",
        "Dominican Republic",
        "Ecuador",
        "Egypt",
        "El Salvador",
        "Equatorial Guinea",
        "Eritrea",
        "Estonia",
        "Eswatini",
        "Swaziland",
        "Ethiopia",
        "Fiji",
        "Finland",
        "France",
        "Gabon",
        "Gambia",
        "Georgia",
        "Germany",
        "Ghana",
        "Greece",
        "Grenada",
        "Guatemala",
        "Guinea",
        "Guinea-Bissau",
        "Guyana",
        "Haiti",
        "Holy See",
        "Honduras",
        "Hungary",
        "Iceland",
        "India",
        "Indonesia",
        "Iran",
        "Iraq",
        "Ireland",
        "Israel",
        "Italy",
        "Jamaica",
        "Japan",
        "Jordan",
        "Kazakhstan",
        "Kenya",
        "Kiribati",
        "Korea (North)",
        "Korea (South)",
        "Kosovo",
        "Kuwait",
        "Kyrgyzstan",
        "Laos",
        "Latvia",
        "Lebanon",
        "Lesotho",
        "Liberia",
        "Libya",
        "Liechtenstein",
        "Lithuania",
        "Luxembourg",
        "Madagascar",
        "Malawi",
        "Malaysia",
        "Maldives",
        "Mali",
        "Malta",
        "Marshall Islands",
        "Mauritania",
        "Mauritius",
        "Mexico",
        "Micronesia",
        "Moldova",
        "Monaco",
        "Mongolia",
        "Montenegro",
        "Morocco",
        "Mozambique",
        "Myanmar (Burma)",
        "Namibia",
        "Nauru",
        "Nepal",
        "Netherlands",
        "New Zealand",
        "Nicaragua",
        "Niger",
        "Nigeria",
        "North Macedonia",
        "Norway",
        "Oman",
        "Pakistan",
        "Palau",
        "Palestine State",
        "Panama",
        "Papua New Guinea",
        "Paraguay",
        "Peru",
        "Philippines",
        "Poland",
        "Portugal",
        "Qatar",
        "Romania",
        "Russia",
        "Rwanda",
        "Saint Kitts and Nevis",
        "Saint Lucia",
        "Saint Vincent and the Grenadines",
        "Samoa",
        "San Marino",
        "Sao Tome and Principe",
        "Saudi Arabia",
        "Senegal",
        "Serbia",
        "Seychelles",
        "Sierra Leone",
        "Singapore",
        "Slovakia",
        "Slovenia",
        "Solomon Islands",
        "Somalia",
        "South Africa",
        "South Sudan",
        "Spain",
        "Sri Lanka",
        "Sudan",
        "Suriname",
        "Sweden",
        "Switzerland",
        "Syria",
        "Tajikistan",
        "Tanzania",
        "Thailand",
        "Timor-Leste",
        "Togo",
        "Tonga",
        "Trinidad and Tobago",
        "Tunisia",
        "Turkey",
        "Turkmenistan",
        "Tuvalu",
        "Uganda",
        "Ukraine",
        "United Arab Emirates",
        "United Kingdom",
        "United States of America",
        "Uruguay",
        "Uzbekistan",
        "Vanuatu",
        "Venezuela",
        "Vietnam",
        "Yemen",
        "Zambia",
        "Zimbabwe"
    ];
$(document).ready(function () {
    ReadProfileOptions();
})

function ReadProfileOptions() {
    $.ajax({
        url: '/Account/GetGenders',
        type: 'get',
        datatype: 'json',
        contentype: 'application/json;charset=utf-8',
        success: function (result) {
            Genders = result.genders;
            Genders.forEach(function (item) {
                var genderBtn = $("<button></button>")
                    .text(item)
                    .addClass("p-1 m-1 background-grass-green zoom-in-hover fw-bold text-white text-truncate rounded-3 border")
                    .on("click", function () {
                        EditGender(item);
                    });
                $("#GendersList").append(genderBtn);
            })
        }
    })
    $.ajax({
        url: '/Account/GetSexualities',
        type: 'get',
        datatype: 'json',
        contentype: 'application/json;charset=utf-8',
        success: function (result) {
            Sexualities = result.sexualities;
            Sexualities.forEach(function (item) {
                var sexualityBtn = $("<button></button>")
                    .text(item)
                    .addClass("p-1 m-1 background-grass-green zoom-in-hover fw-bold text-white text-truncate rounded-3 border")
                    .on("click", function () {
                        EditSexuality(item);
                    });
                $("#SexualityList").append(sexualityBtn);
            })
        }
    })
    $.ajax({
        url: '/Account/GetInterests',
        type: 'get',
        datatype: 'json',
        contentype: 'application/json;charset=utf-8',
        success: function (result) {
            Interests = result.interests;
            Interests.forEach(function (item) {
                var interestsBtn = $("<button></button>")
                    .text(item)
                    .addClass("p-1 m-1 background-grass-green zoom-in-hover fw-bold text-white text-truncate rounded-3 border")
                    .on("click", function () {
                        SelectInterest(item);
                    });
                $("#InterestsList").append(interestsBtn);
            })
        }
    })
    Countries.forEach(function (item) {
        var sexualityBtn = $("<option></option>")
            .text(item)
            .val(item);
        $("#NewCountryInput").append(sexualityBtn);
    })
}

//Name
jQuery(`#BtnName`).click(function () {
    //reset ChangeNameModal
    $("#NewNameErrorLabel").text("");
    $("#NewNameInput").css('border-color', 'lightgray');
    $("#NewNameInput").val("");
    //show Modal
    jQuery('#ChangeNameModal').modal('show');
})
function EditName() {
    //get the data from html
    var formData = new Object();
    formData.newName = $("#NewNameInput").val();

    //send the data
    $.ajax({
        url: '/Account/EditName',
        data: formData,
        type: 'post',
        //if the call was a success, check if the action was a success
        success: function (response) {
            //if yes, reload the page
            if (response.success) {
                window.location.reload(true);
            }
            //if no, show the errors
            else {
                $("#NewNameInput").css('border-color','Red');
                $("#NewNameErrorLabel").text(response.errors.join(", "));
            }
        },
        //if the call couldn't be made, show a generic error
        error: function () {
            $("#NewNameErrorLabel").text("An error occurred while changing the name.");
        }
    });
}
function HideNameModal() {
    $('#ChangeNameModal').modal('hide');
}

//Age
jQuery(`#BtnAge`).click(function () {
    //reset ChangeNameModal
    $("#NewAgeErrorLabel").text("");
    $("#NewAgeInput").css('border-color', 'lightgray');
    $("#NewAgeInput").val("");
    //show Modal
    jQuery('#ChangeAgeModal').modal('show');
})
function EditAge() {
    //get the data from html
    var formData = new Object();
    formData.newAge = $("#NewAgeInput").val();

    //send the data
    $.ajax({
        url: '/Account/EditAge',
        data: formData,
        type: 'post',
        //if the call was a success, check if the action was a success
        success: function (response) {
            //if yes, reload the page
            if (response.success) {
                window.location.reload(true);
            }
            //if no, show the errors
            else {
                $("#NewAgeInput").css('border-color', 'Red');
                $("#NewAgeErrorLabel").text(response.errors.join(", "));
            }
        },
        //if the call couldn't be made, show a generic error
        error: function () {
            $("#NewAgeErrorLabel").text("An error occurred while changing the name.");
        }
    });
}
function HideAgeModal() {
    $('#ChangeAgeModal').modal('hide');
}

//gender
jQuery(`#BtnGender`).click(function () {
    //reset ChangeNameModal
    $("#NewGenderErrorLabel").text("");
    jQuery('#ChangeGenderModal').modal('show');
})
function EditGender(newGender) {
    var formData = new Object();
    formData.selectedGender = newGender;
    //send the data
    $.ajax({
        url: '/Account/EditGender',
        data: formData,
        type: 'post',
        //if the call was a success, check if the action was a success
        success: function (response) {
            //if yes, reload the page
            if (response.success) {
                window.location.reload(true);
            }
            //if no, show the errors
            else {
                $("#NewGenderInput").css('border-color', 'Red');
                $("#NewGenderErrorLabel").text(response.errors.join(", "));
            }
        },
        //if the call couldn't be made, show a generic error
        error: function () {
            $("#NewGenderErrorLabel").text("An error occurred while changing the name.");
        }
    });
}
function HideGenderModal() {
    $('#ChangeGenderModal').modal('hide');
}

//sexuality
jQuery(`#BtnSexuality`).click(function () {
    //reset ChangeNameModal
    $("#NewSexualityErrorLabel").text("");
    jQuery('#ChangeSexualityModal').modal('show');
})
function EditSexuality(newSexuality) {
    var formData = new Object();
    formData.selectedSexuality = newSexuality;
    //send the data
    $.ajax({
        url: '/Account/EditSexuality',
        data: formData,
        type: 'post',
        //if the call was a success, check if the action was a success
        success: function (response) {
            //if yes, reload the page
            if (response.success) {
                window.location.reload(true);
            }
            //if no, show the errors
            else {
                $("#NewSexualityInput").css('border-color', 'Red');
                $("#NewSexualityErrorLabel").text(response.errors.join(", "));
            }
        },
        //if the call couldn't be made, show a generic error
        error: function () {
            $("#NewSexualityErrorLabel").text("An error occurred while changing the name.");
        }
    });
}
function HideSexualityModal() {
    $('#ChangeSexualityModal').modal('hide');
}

//interests
var selectedInterests = [];
jQuery(`#BtnInterests`).click(function () {
    //reset ChangeNameModal
    $("#NewInterestsErrorLabel").text("");
    jQuery('#ChangeInterestsModal').modal('show');
})
function SelectInterest(interest)
{

}
function EditInterests() {
    var formData = new Object();
    formData.selectedSexuality = newSexuality;
    //send the data
    $.ajax({
        url: '/Account/EditInterests',
        data: formData,
        type: 'post',
        //if the call was a success, check if the action was a success
        success: function (response) {
            //if yes, reload the page
            if (response.success) {
                window.location.reload(true);
            }
            //if no, show the errors
            else {
                $("#NewInterestsInput").css('border-color', 'Red');
                $("#NewInterestsErrorLabel").text(response.errors.join(", "));
            }
        },
        //if the call couldn't be made, show a generic error
        error: function () {
            $("#NewInterestsErrorLabel").text("An error occurred while changing the name.");
        }
    });
}
function HideInterestsModal() {
    $('#ChangeInterestsModal').modal('hide');
}

//location
jQuery(`#BtnLocation`).click(function () {
    //reset ChangeNameModal
    $("#NewLocationErrorLabel").text("");
    jQuery('#ChangeLocationModal').modal('show');
})
function EditLocation() {
    var formData = new Object();
    formData.country = $("#NewCountryInput").val();
    formData.city = "";
    //send the data
    $.ajax({
        url: '/Account/EditLocation',
        data: formData,
        type: 'post',
        //if the call was a success, check if the action was a success
        success: function (response) {
            //if yes, reload the page
            if (response.success) {
                window.location.reload(true);
            }
            //if no, show the errors
            else {
                $("#NewLocationErrorLabel").text(response.errors.join(", "));
            }
        },
        //if the call couldn't be made, show a generic error
        error: function () {
            $("#NewLocationErrorLabel").text("An error occurred while changing the name.");
        }
    });
}
function HideSexualityModal() {
    $('#ChangeLocationModal').modal('hide');
}