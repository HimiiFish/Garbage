using System;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x02000005 RID: 5
public class CoursePanel : MonoBehaviour
{
	// Token: 0x06000009 RID: 9 RVA: 0x00002108 File Offset: 0x00000308
	private void OnEnable()
	{
		this.btn1.gameObject.SetActive(true);
		this.btn2.gameObject.SetActive(true);
		this.btn3.gameObject.SetActive(true);
		this.btn1.onClick.AddListener(delegate()
		{
			this.btn1.gameObject.SetActive(false);
		});
		this.btn2.onClick.AddListener(delegate()
		{
			this.btn2.gameObject.SetActive(false);
		});
		this.btn3.onClick.AddListener(delegate()
		{
			this.btn3.gameObject.SetActive(false);
			base.gameObject.SetActive(false);
		});
	}

	// Token: 0x04000004 RID: 4
	public Button btn1;

	// Token: 0x04000005 RID: 5
	public Button btn2;

	// Token: 0x04000006 RID: 6
	public Button btn3;
}
