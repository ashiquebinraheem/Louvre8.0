
var gvReportExportType, gvRow;

$(function () {

	//#region Ajax


	PostForm = function (evt, apiName, formId, successFun, msgDivId = "div_message", msgId = "msg") {

		evt.preventDefault();

		var form = $(formId)[0];

		var formData = new FormData(form);

		$(".validation-summary-errors").show();

		if (!$(formId).valid()) return false;

		$(".validation-summary-errors").hide();

		$(':button').prop('disabled', true);
		$('#please-wait').css('display', 'flex');

		$.ajax({
			url: apiName,
			processData: false,
			contentType: false,
			data: formData,
			type: 'POST'
		}).done(function (result) {

			$(':button').prop('disabled', false);
			$('#please-wait').css('display', 'none');

			if (result.ResponseCode < 0) {
				errorMessage(result.ResponseMessage, result.ResponseTitle);
				//console.log(result.ResponseErrorDescription);
				return;
			}
			window[successFun](result);
		}).fail(function (a, b, c) {
			//errorMessage();
			errorMessage(a.responseJSON.responseTitle, a.responseJSON.responseMessage);
			//swal_ajax('error');
			$(':button').prop('disabled', false);
			$('#please-wait').css('display', 'none');
		});
	}

	PostFormWithoutEvent = function (apiName, formId, successFun, msgDivId = "div_message", msgId = "msg") {

		$(':button').prop('disabled', true);

		var form = $(formId)[0];

		//TimeZoneMinutes
		//$('<input>').attr({type: 'hidden',id: 'TimeZoneMinutes',name: 'TimeZoneMinutes',value: getTimeZone()}).appendTo('form');

		var formData = new FormData(form);
		$.ajax({
			url: apiName,
			processData: false,
			contentType: false,
			data: formData,
			type: 'POST'
		}).done(function (result) {
			$(':button').prop('disabled', false);
			if (result.ResponseCode < 0) {
				errorMessage(result.ResponseMessage, result.ResponseTitle);
				//console.log(result.ResponseErrorDescription);
				return;
			}
			window[successFun](result);
		}).fail(function (a, b, c) {
			errorMessage(a.responseJSON.responseTitle, a.responseJSON.responseMessage);
			$(':button').prop('disabled', false);
		});
	}

	GetAPI = function (apiName, successFun, reference) {
		$.ajax({
			type: "GET",
			url: apiName,
			contentType: "application/json; charset=utf-8",
			success: function (data) {
				window[successFun](data, reference);
			},
			error: function (a, b, c) {
				errorMessage(a.responseJSON.responseTitle, a.responseJSON.responseMessage);
				//$("#msg").html(XMLHttpRequest.responseJSON.Message);
				//$("#div_message").removeClass().addClass('div_message alert alert-danger').show().delay(10000).hide("slow");
			}
		});
	};

	PostAPI = function (apiName, paramArray, successFun, reference) {
		$.ajax({
			type: "POST",
			url: apiName,
			data: JSON.stringify(paramArray),
			contentType: "application/json; charset=utf-8",
			dataType: "json",
			success: function (data) {
				window[successFun](data, reference);
			},
			error: function (XMLHttpRequest, textStatus, errorThrown) {
				//$("#msg").html(XMLHttpRequest.responseJSON.Message);
				//$("#div_message").removeClass().addClass('div_message alert alert-danger').show().delay(10000).hide("slow");
			}
		});
	};

	//#endregion

	//#region Message

	errorMessage = function (msg = "Oops..Something went wrong!", title = "") {
		if (title == "") {
			const Toast = Swal.mixin({
				toast: true,
				position: 'top-end',
				showConfirmButton: false,
				timer: 3000,
				timerProgressBar: true,
				onOpen: (toast) => {
					toast.addEventListener('mouseenter', Swal.stopTimer)
					toast.addEventListener('mouseleave', Swal.resumeTimer)
				}
			})

			Toast.fire({
				icon: 'error',
				title: msg
			})
		}
		else {
			Swal.fire({
				icon: 'error',
				title: title,
				text: msg,
			})
		}
	}

	successMessage = function (msg = "You're done!!", title = "", redirectURL = "") {

		///reference : https://sweetalert2.github.io/#frameworks-integrations
		if (title == "") {
			Swal.fire({
				position: 'top-end',
				icon: 'success',
				title: `${msg}`,
				showConfirmButton: false,
				timer: 1500
			}).then((result) => {
				if (redirectURL != "" && redirectURL != "/") {
					window.location.href = redirectURL;
				}
			})
		}
		else {
			Swal.fire({
				title: `<strong>${title}</strong>`,
				icon: 'success',
				html: msg,
				showCloseButton: false,
				showCancelButton: false,
				focusConfirm: false,
				confirmButtonText:
					'<i class="fa fa-thumbs-up"></i> Great!',
				confirmButtonAriaLabel: 'Thumbs up, great!',
			}).then((result) => {
				if (redirectURL != "" && redirectURL != "/") {
					window.location.href = redirectURL;
				}
			})
		}
	}

	showMessage = function (msgDesciption, msg, icon, redirectURL = "") {

		///reference : https://sweetalert2.github.io/#frameworks-integrations

		Swal.fire({
			title: `<strong>${msg}</strong>`,
			icon: icon,
			html: msgDesciption,
			showCloseButton: false,
			showCancelButton: false,
			focusConfirm: false,
			confirmButtonText:
				'<i class="fa fa-thumbs-up"></i> Okay!',
			confirmButtonAriaLabel: 'Thumbs up, great!',
			//cancelButtonText:
			//	'<i class="fa fa-thumbs-down"></i>',
			//cancelButtonAriaLabel: 'Thumbs down'
		}).then((result) => {
			if (redirectURL != "") {
				window.location.href = '/' + redirectURL;
			}
		})
	}




	function swal_ajax(type) {
		switch (type) {
			case 'load':
				swal.fire({
					title: '',
					html: '<div class="save_loading"><svg viewBox="0 0 140 140" width="140" height="140"><g class="outline"><path d="m 70 28 a 1 1 0 0 0 0 84 a 1 1 0 0 0 0 -84" stroke="rgba(0,0,0,0.1)" stroke-width="4" fill="none" stroke-linecap="round" stroke-linejoin="round"></path></g><g class="circle"><path d="m 70 28 a 1 1 0 0 0 0 84 a 1 1 0 0 0 0 -84" stroke="#71BBFF" stroke-width="4" fill="none" stroke-linecap="round" stroke-linejoin="round" stroke-dashoffset="200" stroke-dasharray="300"></path></g></svg></div><div><h4>Save in progress...</h4></div>',
					showConfirmButton: false,
					allowOutsideClick: false
				});
				break;
			case 'error':
				setTimeout(function () {
					$('#swal2-content').html('<div class="sa"><div class="sa-error"><div class="sa-error-x"><div class="sa-error-left"></div><div class="sa-error-right"></div></div><div class="sa-error-placeholder"></div><div class="sa-error-fix"></div></div></div><div><h4>An error has occurred; please contact web support for assistance.</h4></div><button class="confirm swal-close" style="display: inline-block; border-left-color: rgb(48, 133, 214); border-right-color: rgb(48, 133, 214);">OK</button>');
				}, 1000);
				$('.swal-close').on('click', function () { swal.closeModal(); });
				break;
			case 'success':
				setTimeout(function () {
					$('#swal2-content').html('<div class="sa"><div class="sa-success"><div class="sa-success-tip"></div><div class="sa-success-long"></div><div class="sa-success-placeholder"></div><div class="sa-success-fix"></div></div></div><div><h4>Save successful!</h4></div>');
				}, 1000);
				setTimeout(function () {
					swal.close(true);
				}, 2000);
				break;
		}
	}


	//#endregion

	//#region Date Formate
	Date.prototype.toDateInputValue = (function () {
		var local = new Date(this);
		local.setMinutes(this.getMinutes() - this.getTimezoneOffset());
		return local.toJSON().slice(0, 10);
	});
	//#endregion

	//#region Select box and List

	fillCombo = function (name, valuefield, textfield, data, defaultitem, neednew) {
		var cmb = $("#" + name);
		cmb.empty();

		if (defaultitem != '') {
			cmb.append($("<option/>").val(null).text(defaultitem)); // use .text()
		}

		if (data != null) {
			$.each(data, function () {
				// Use .text() for user-supplied display text
				cmb.append($("<option/>").val(this[valuefield]).text(this[textfield]));
			});
		}

		if (neednew) {
			cmb.append($("<option/>").val(-1).text("<- New ->")); // safe
		}
	}



	setComboByNameAndValue = function (name, valuefield, textfield, data, selectedValue, defaultitem) {
		var cmb = $(`select[name="${name}"]`);
		cmb.empty();
		if (defaultitem == '<-Select One->') {
			cmb.append($("<option/>").val(null).text(defaultitem));
		}
		else if (defaultitem != '') {
			cmb.append($("<option/>").val(0).text(defaultitem));
		}

		if (data != null) {
			$.each(data, function () {
				cmb.append($("<option/>").val(this[valuefield]).text(this[textfield]));
			});
		}
		$(cmb).val(selectedValue);
	}

	fillSuggestion = function (name, data) {
		var cmb = $("#" + name);
		cmb.empty();
		for (var i = 0; i < data.length; i++) {
			cmb.append($('<option/>').val(data[i]).text(data[i]));
		}
	}

	fillFieldSuggestion = function (name, data, fieldName) {
		var cmb = $("#" + name);
		cmb.empty();
		for (var i = 0; i < data.length; i++) {
			cmb.append($('<option/>').val(data[i][fieldName]).text(data[i][fieldName]));
		}
	}

	$("select[readonly]").on("focus mousedown mouseup click", function (e) {
		e.preventDefault();
		e.stopPropagation();
	});

	//#endregion

	//#region Number Textbox

	getDecimalText = function (textboxName) {
		var value = 0;
		if ($("#" + textboxName).val() != "" && $("#" + textboxName).val() != undefined)
			value = parseFloat($("#" + textboxName).val());

		return value;
	}

	$(document).on("keypress", ".decimal", function (event) {
		return isDecimal(event, this);
	});

	$(document).on("keypress", ".integer", function (event) {
		return isInteger(event, this);
	});

	$(document).on("keypress", ".negativenumber", function (event) {
		return isNegativeNumber(event, this);
	});

	function isDecimal(evt, element) {
		var charcode = evt.which ? evt.which : event.KeyCode;
		if (charcode == 8 || (charcode == 46 && $(element).val().indexOf('.') == -1) || event.keyCode == 37 || event.keyCode == 39) {
			return true;
		}
		else if (charcode < 48 || charcode > 57) {
			return false;
		}
		else return true;
	}

	function isInteger(evt, element) {
		var charcode = evt.which ? evt.which : event.KeyCode;
		if (charcode == 8 || event.keyCode == 37 || event.keyCode == 39) {
			return true;
		}
		else if (charcode < 48 || charcode > 57) {
			return false;
		}
		else return true;
	}

	function isNegativeNumber(evt, element) {
		var charcode = evt.which ? evt.which : event.KeyCode;
		if (charcode == 8 || (charcode == 45 && $(element).val().indexOf('-') == -1) || (charcode == 46 && $(element).val().indexOf('.') == -1) || event.keyCode == 37 || event.keyCode == 39) {
			return true;
		}
		else if (charcode < 48 || charcode > 57) {
			return false;
		}
		else return true;
	}

	//#endregion

	//#region Model State Valid Check

	isModelStateValid = function (controls) {
		var missing = -1;
		for (i = 0; i < controls.length; i++) {
			if (controls[i].val() == '' || controls[i].val() == undefined || controls[i].val() == '-1') {
				controls[i].addClass('input-validation-error');

				if (missing == -1)
					missing = i;
			}
			else {
				controls[i].removeClass('input-validation-error');
			}
		}
		if (missing != -1) {
			cntr[missing].focus();
			return false;
		}
		return true;
	}

	//#endregion

	//#region Array Functions

	getRowIndexById = function (dataCollection, idFieldName, value) {

		for (var i = 0; i < dataCollection.length; i++) {
			if (dataCollection[i][idFieldName] == value) {
				return i;
				break;
			}
		}
		return 0;
	}

	getRowByFieldNameValue = function (dataCollection, idFieldName, value) {

		for (var i = 0; i < dataCollection.length; i++) {
			if (dataCollection[i][idFieldName] == value) {
				return dataCollection[i];
				break;
			}
		}
		return 0;
	}

	//#endregion

	//#region Image Viewer

	viewImage = function (input, viewerId) {
		if (input.files && input.files[0]) {
			$("#" + viewerId).show();

			var reader = new FileReader();
			reader.onload = function (e) {
				$("#" + viewerId).attr('src', e.target.result);
			};
			reader.readAsDataURL(input.files[0]);
		}
		else {
			$("#" + viewerId).hide();
		}
	}


	viewVideo = function (input, viewerId) {
		if (input.files && input.files[0]) {
			$("#" + viewerId).show();
			document.getElementById(viewerId).setAttribute("src", URL.createObjectURL(input.files[0]));
		}
		else {
			$("#" + viewerId).hide();
		}

	}

	viewAudio = function (input, viewerId) {
		if (input.files && input.files[0]) {
			$("#" + viewerId).show();
			document.getElementById(viewerId).setAttribute("src", URL.createObjectURL(input.files[0]));
		}
		else {
			$("#" + viewerId).hide();
		}

		//var audio = $("#player");
		//$("#ogg_src").attr("src", sourceUrl);
		///****************/
		//audio[0].pause();
		//audio[0].load();//suspends and restores all audio element

		////audio[0].play(); changed based on Sprachprofi's comment below
		//audio[0].oncanplaythrough = audio[0].play();
		///****************/
	}

	hideImgBox = function (input, viewerId) {
		if (input.files && input.files[0]) {
			$("#" + viewerId).hide();
		}
	}

	showImage = function (input, viewerId) {
		if (input.files && input.files[0]) {
			$("#" + viewerId).show();

			var reader = new FileReader();
			reader.onload = function (e) {
				$("#" + viewerId).css('background-image', `url('${e.target.result}')`);
			};
			reader.readAsDataURL(input.files[0]);
		}
	}

	showURLImage = function (mediasrc, viewerId) {
		$("#" + viewerId).show();
		$("#" + viewerId).css('background-image', `url('${mediasrc.value}')`);
	}


	const imageExtensions = ['jpg', 'jpeg', 'png', 'gif', 'bmp', 'webp', 'tiff'];

	showImageInPopup = function (viewerControl) {
		var viewerId = $(viewerControl).attr('id');
		var bg = $(`#${viewerId}`).css("background-image");
		var nameArr = bg.split(',');
		var bg_img = nameArr[0];

		bg_img = bg_img.replace('url(', '').replace(')', '').replace(/\"/gi, "");
		const extension = bg_img.split('.').pop().toLowerCase();

		$(`#popupPdf`).hide();
		$(`#popupImage`).hide();


		if (bg_img.includes(".pdf")) {
			$(`#popupPdf`).attr('src', bg_img);
			$(`#popupPdf`).show();
		}
		else if (!imageExtensions.includes(extension)) {
			//event.preventDefault(); // Prevent the default action of the anchor tag
			//var url = $(this).attr('href'); // Get the URL from the href attribute of the link
			window.open(bg_img, '_blank');
			$("#photViewer").modal('hide');
		}
		else {
			$(`#popupImage`).attr('src', bg_img);
			$(`#popupImage`).show();
		}
	}

	showImageInPopupPdf = function (viewerControl) {
		var viewerId = $(viewerControl).attr('id');
		var bg_img = $(`#${viewerId}`).css("background-image");

		bg_img = bg_img.replace('url(', '').replace(')', '').replace(/\"/gi, "");

		var comaIndex = bg_img.indexOf(',');
		if (comaIndex != -1) {
			bg_img = bg_img.substr(0, comaIndex);
		}
		$(`#popupPdf`).hide();
		$(`#popupImage`).hide();
		if (bg_img.includes(".pdf")) {
			$(`#popupPdf`).attr('src', bg_img);
			$(`#popupPdf`).show();
		}
		else {
			$(`#popupImage`).attr('src', bg_img);
			$(`#popupImage`).show();
		}
	}

	viewThumbImage = function (src) {
		$("#popupImage").attr('src', src);
		$("#photViewer").modal('show');
	}


	//#endregion

	//#region Time Zone

	getTimeZone = function () {
		var d = new Date()
		return d.getTimezoneOffset() * -1;
	};

	setTimezoneCookie = function () {
		$.cookie("timezoneoffset", getTimeZone());
	}

	//#endregion

	//#region Report Export

	exportReport = function (divExportReport, reportName = "report") {

		$("#divExportReport").html(divExportReport);
		$("#divExportReport").show();
		if (gvReportExportType == "excel")
			excelExport('divExportReport', reportName);
		else if (gvReportExportType == "csv")
			csvExport('divExportReport', reportName);
		else
			pdfExport('divExportReport', reportName);
		$("#divExportReport").hide();

	}

	csvExport = function (gridId, reportName = "report") {
		$('#' + gridId).tableExport({ type: 'csv', fileName: reportName });
	}

	excelExport = function (gridId, reportName = "report") {
		//$('#' + gridId).tableExport({ type: 'excel', fileName: reportName });
		$('#' + gridId).tableExport({ type: 'excel' });
	}

	pdfExport = function (gridId, reportName = "report") {
		$("#" + gridId).tableExport({
			type: 'pdf',
			fileName: reportName,
			jspdf: {
				orientation: 'l',
				format: 'a3',
				margins: { left: 10, right: 10, top: 20, bottom: 20 },
				autotable: {
					styles: {
						fillColor: 'inherit',
						textColor: 'inherit'
					},
					tableWidth: 'auto'
				}
			}
		});
	}

	//#endregion

	//#region Table Functions

	addTableRow = function (tableId, modelName, fields) {
		var slNo = $(`#${tableId} tbody tr`).length;
		var newRow = "<tr>";
		var hiddenFields = "";

		cntr = [];
		$.each(fields, function () {
			if (this['Required'] == "required") {
				cntr.push($('#' + this['ControlName']));
			}
		});
		if (!isModelStateValid(cntr))
			return;

		$.each(fields, function () {

			if (this['FieldType'] == "select") {
				newRow += `<td><select class='form-control' name='${modelName}[${slNo}].${this['FieldName']}' ${this['Required']}></select></td>`;
			}
			else if (this['FieldType'] == "slno") {
				newRow += `<td class="text-center"><label name="${modelName}[${slNo}].RowSlNo">${slNo + 1}</label></td>`;
			}
			else if (this['FieldType'] == "hidden") {
				hiddenFields += `<input type="hidden" name="${modelName}[${slNo}].${this['FieldName']}" value="` + $('#' + this['ControlName']).val() + `"/>`;
			}
			else if (this['FieldType'] == "checkbox") {

				var checked = "", checkedValue = "";
				if ($('#' + this['ControlName']).is(":checked")) {
					checked = "checked";
					checkedValue = "true";
				}
				newRow += `<td><input type="${this['FieldType']}" class="form-control" name="${modelName}[${slNo}].${this['FieldName']}" ${checked} value="${checkedValue}"></td>`;
			}
			else if (this['FieldType'] == "file")
			{
				newRow += `<td><input type="${this['FieldType']}" class="form-control ${this['Class']}" name="${modelName}[${slNo}].${this['FieldName']}" ${this['Required']}  ${this['Attributes']}></td>`;
				
			}
			else {
				newRow += `<td><input type="${this['FieldType']}" class="form-control ${this['Class']}" name="${modelName}[${slNo}].${this['FieldName']}" value="` + $('#' + this['ControlName']).val() + `" ${this['Required']}  ${this['Attributes']}></td>`;
			}
		});

		newRow += `<td>${hiddenFields}
                         <a onclick="removeTableRow(this,$(this).closest('table').attr('id'))"><span class="fa fa-trash"></span></a>
                        </td>`

		newRow += "</tr>";
		$(`#${tableId} tbody`).append(newRow);

		$.each(fields, function () {

			if (this['FieldType'] == "select") {

				var options = $(`#${this['ControlName']} > option`).clone();
				$(`[name="${modelName}[${slNo}].${this['FieldName']}"]`).append(options);
				$(`[name="${modelName}[${slNo}].${this['FieldName']}"]`).val($(`#${this['ControlName']}`).val());

				$(`[name="${modelName}[${slNo}].${this['FieldName']}"]> option[value='-1']`).remove();
			}
			else if (this['FieldType'] == "checkbox") {
				$('#' + this['ControlName']).prop("checked", false);
			}
			else if (this['FieldType'] == "file") {
				$(`input[name="${modelName}[${slNo}].${this['FieldName']}"]`)[0].files = $('#' + this['ControlName'])[0].files;
			}
		});

		$(`#${tableId} tfoot tr`).find("input:text,select").each(function () {
			this.value = "";
		});
	}

	removeTableRow = function (selector, tableId) {
		if ($(`#${tableId} tbody tr`).length > 0) {
			$(selector).closest('tr').remove();
			$(`#${tableId} tbody tr`).each(function (i, val) {
				$(this).find('label[name$=".RowSlNo"]').html(i + 1);
				$(this).find("input, textarea, select").each(function () {
					$(this).attr('name', $(this).attr('name').replace(/\[\d+\]/, "[" + (i) + "]"));
				});
			});
		}
	}

	//#endregion

	//#region Image array save Functions

	setHasImageField = function (cnt, imageArrayId, hasFileFieldId) {
		for (i = 0; i < cnt; i++) {
			if (document.getElementById(`${imageArrayId}_${i}`).files.length == 0) {
				$(`#${hasFileFieldId}_${i}`).val(false);
			}
			else {
				$(`#${hasFileFieldId}_${i}`).val(true);
			}
		}
	}

	//#endregion

	//#region Clear Form

	clearForm = function (formId = "formBasic") {
		$(':input', '#' + formId)
			.not(':button, :submit, :reset, :hidden')
			.val('')
			.prop('checked', false)
			.prop('selected', false);
	}

	clearImage = function (viewerId) {
		$("#" + viewerId).css('background-image', ``);
	};

	clearImages = function (viewerId, cnt) {
		for (i = 0; i < cnt; i++) {
			$(`#${viewerId}_${i}`).css('background-image', ``);
		}
	};


	//#endregion



	
});
